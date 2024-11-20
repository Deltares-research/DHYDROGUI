import string
import math
import struct
import sys
import uuid
from sys import version_info
if version_info < ( 3, 0 ):
    from cStringIO import StringIO
else:
    from io import StringIO

if version_info < ( 3, 0 ):
    BytesIO = StringIO
else:
    from io import BytesIO

from . import filters
from . import utils
import warnings
import codecs
from .generic import *
from .utils import readNonWhitespace, readUntilWhitespace, ConvertFunctionsToVirtualList
from .utils import isString, b_, u_, ord_, chr_, str_, formatWarning

if version_info < ( 2, 4 ):
   from sets import ImmutableSet as frozenset

if version_info < ( 2, 5 ):
    from md5 import md5
else:
    from hashlib import md5
import uuid


class PdfFileReader(object):
    """
    Initializes a PdfFileReader object.  This operation can take some time, as
    the PDF stream's cross-reference tables are read into memory.

    :param stream: A File object or an object that supports the standard read
        and seek methods similar to a File object. Could also be a
        string representing a path to a PDF file.
    :param bool strict: Determines whether user should be warned of all
        problems and also causes some correctable problems to be fatal.
        Defaults to ``True``.
    :param warndest: Destination for logging warnings (defaults to
        ``sys.stderr``).
    :param bool overwriteWarnings: Determines whether to override Python's
        ``warnings.py`` module with a custom implementation (defaults to
        ``True``).
    """
    def __init__(self, stream, strict=True, warndest = None, overwriteWarnings = True):
        if overwriteWarnings:
            # have to dynamically override the default showwarning since there are no
            # public methods that specify the 'file' parameter
            def _showwarning(message, category, filename, lineno, file=warndest, line=None):
                if file is None:
                    file = sys.stderr
                try:
                    file.write(formatWarning(message, category, filename, lineno, line))
                except IOError:
                    pass
            warnings.showwarning = _showwarning
        self.strict = strict
        self.flattenedPages = None
        self.resolvedObjects = {}
        self.xrefIndex = 0
        self._pageId2Num = None # map page IndirectRef number to Page Number
        if hasattr(stream, 'mode') and 'b' not in stream.mode:
            warnings.warn("PdfFileReader stream/file object is not in binary mode. It may not be read correctly.", utils.PdfReadWarning)
        if isString(stream):
            fileobj = open(stream, 'rb')
            stream = BytesIO(b_(fileobj.read()))
            fileobj.close()
        self.read(stream)
        self.stream = stream

        self._override_encryption = False

    def getDocumentInfo(self):
        """
        Retrieves the PDF file's document information dictionary, if it exists.
        Note that some PDF files use metadata streams instead of docinfo
        dictionaries, and these metadata streams will not be accessed by this
        function.

        :return: the document information of this PDF file
        :rtype: :class:`DocumentInformation<pdf.DocumentInformation>` or ``None`` if none exists.
        """
        if "/Info" not in self.trailer:
            return None
        obj = self.trailer['/Info']
        retval = DocumentInformation()
        retval.update(obj)
        return retval

    documentInfo = property(lambda self: self.getDocumentInfo(), None, None)
    """Read-only property that accesses the :meth:`getDocumentInfo()<PdfFileReader.getDocumentInfo>` function."""
    
    
    
    def getObject(self, indirectReference):
        debug = False
        if debug: print(("looking at:", indirectReference.idnum, indirectReference.generation))
        retval = self.cacheGetIndirectObject(indirectReference.generation,
                                                indirectReference.idnum)
        if retval != None:
            return retval
        if indirectReference.generation == 0 and \
                        indirectReference.idnum in self.xref_objStm:
            retval = self._getObjectFromStream(indirectReference)
        elif indirectReference.generation in self.xref and \
                indirectReference.idnum in self.xref[indirectReference.generation]:
            start = self.xref[indirectReference.generation][indirectReference.idnum]
            if debug: print(("  Uncompressed Object", indirectReference.idnum, indirectReference.generation, ":", start))
            self.stream.seek(start, 0)
            idnum, generation = self.readObjectHeader(self.stream)
            if idnum != indirectReference.idnum and self.xrefIndex:
                # Xref table probably had bad indexes due to not being zero-indexed
                if self.strict:
                    raise utils.PdfReadError("Expected object ID (%d %d) does not match actual (%d %d); xref table not zero-indexed." \
                                     % (indirectReference.idnum, indirectReference.generation, idnum, generation))
                else: pass # xref table is corrected in non-strict mode
            elif idnum != indirectReference.idnum:
                # some other problem
                raise utils.PdfReadError("Expected object ID (%d %d) does not match actual (%d %d)." \
                                         % (indirectReference.idnum, indirectReference.generation, idnum, generation))
            assert generation == indirectReference.generation
            retval = readObject(self.stream, self)

            # override encryption is used for the /Encrypt dictionary
            if not self._override_encryption and self.isEncrypted:
                # if we don't have the encryption key:
                if not hasattr(self, '_decryption_key'):
                    raise utils.PdfReadError("file has not been decrypted")
                # otherwise, decrypt here...
                import struct
                pack1 = struct.pack("<i", indirectReference.idnum)[:3]
                pack2 = struct.pack("<i", indirectReference.generation)[:2]
                key = self._decryption_key + pack1 + pack2
                assert len(key) == (len(self._decryption_key) + 5)
                md5_hash = md5(key).digest()
                key = md5_hash[:min(16, len(self._decryption_key) + 5)]
                retval = self._decryptObject(retval, key)
        else:
            warnings.warn("Object %d %d not defined."%(indirectReference.idnum,
                        indirectReference.generation), utils.PdfReadWarning)
            #if self.strict:
            raise utils.PdfReadError("Could not find object.")
        self.cacheIndirectObject(indirectReference.generation,
                    indirectReference.idnum, retval)
        return retval

    def _decryptObject(self, obj, key):
        if isinstance(obj, ByteStringObject) or isinstance(obj, TextStringObject):
            obj = createStringObject(utils.RC4_encrypt(key, obj.original_bytes))
        elif isinstance(obj, StreamObject):
            obj._data = utils.RC4_encrypt(key, obj._data)
        elif isinstance(obj, DictionaryObject):
            for dictkey, value in list(obj.items()):
                obj[dictkey] = self._decryptObject(value, key)
        elif isinstance(obj, ArrayObject):
            for i in range(len(obj)):
                obj[i] = self._decryptObject(obj[i], key)
        return obj

    def readObjectHeader(self, stream):
        # Should never be necessary to read out whitespace, since the
        # cross-reference table should put us in the right spot to read the
        # object header.  In reality... some files have stupid cross reference
        # tables that are off by whitespace bytes.
        extra = False
        utils.skipOverComment(stream)
        extra |= utils.skipOverWhitespace(stream); stream.seek(-1, 1)
        idnum = readUntilWhitespace(stream)
        extra |= utils.skipOverWhitespace(stream); stream.seek(-1, 1)
        generation = readUntilWhitespace(stream)
        obj = stream.read(3)
        readNonWhitespace(stream)
        stream.seek(-1, 1)
        if (extra and self.strict):
            #not a fatal error
            warnings.warn("Superfluous whitespace found in object header %s %s" % \
                          (idnum, generation), utils.PdfReadWarning)
        return int(idnum), int(generation)

    def cacheGetIndirectObject(self, generation, idnum):
        debug = False
        out = self.resolvedObjects.get((generation, idnum))
        if debug and out: print(("cache hit: %d %d"%(idnum, generation)))
        elif debug: print(("cache miss: %d %d"%(idnum, generation)))
        return out

    def cacheIndirectObject(self, generation, idnum, obj):
        # return None # Sometimes we want to turn off cache for debugging.
        if (generation, idnum) in self.resolvedObjects:
            msg = "Overwriting cache for %s %s"%(generation, idnum)
            if self.strict: raise utils.PdfReadError(msg)
            else:           warnings.warn(msg)
        self.resolvedObjects[(generation, idnum)] = obj
        return obj

    def read(self, stream):
        debug = False
        if debug: print(">>read", stream)
        # start at the end:
        stream.seek(-1, 2)
        if not stream.tell():
            raise utils.PdfReadError('Cannot read an empty file')
        last1K = stream.tell() - 1024 + 1 # offset of last 1024 bytes of stream
        line = b_('')
        while line[:5] != b_("%%EOF"):
            if stream.tell() < last1K:
                raise utils.PdfReadError("EOF marker not found")
            line = self.readNextEndLine(stream)
            if debug: print("  line:",line)

        # find startxref entry - the location of the xref table
        line = self.readNextEndLine(stream)
        try:
            startxref = int(line)
        except ValueError:
            # 'startxref' may be on the same line as the location
            if not line.startswith(b_("startxref")):
                raise utils.PdfReadError("startxref not found")
            startxref = int(line[9:].strip())
            warnings.warn("startxref on same line as offset")
        else:
            line = self.readNextEndLine(stream)
            if line[:9] != b_("startxref"):
                raise utils.PdfReadError("startxref not found")

        # read all cross reference tables and their trailers
        self.xref = {}
        self.xref_objStm = {}
        self.trailer = DictionaryObject()
        while True:
            # load the xref table
            stream.seek(startxref, 0)
            x = stream.read(1)
            if x == b_("x"):
                # standard cross-reference table
                ref = stream.read(4)
                if ref[:3] != b_("ref"):
                    raise utils.PdfReadError("xref table read error")
                readNonWhitespace(stream)
                stream.seek(-1, 1)
                firsttime = True; # check if the first time looking at the xref table
                while True:
                    num = readObject(stream, self)
                    if firsttime and num != 0:
                         self.xrefIndex = num
                         if self.strict:
                            warnings.warn("Xref table not zero-indexed. ID numbers for objects will be corrected.", utils.PdfReadWarning)
                            #if table not zero indexed, could be due to error from when PDF was created
                            #which will lead to mismatched indices later on, only warned and corrected if self.strict=True
                    firsttime = False
                    readNonWhitespace(stream)
                    stream.seek(-1, 1)
                    size = readObject(stream, self)
                    readNonWhitespace(stream)
                    stream.seek(-1, 1)
                    cnt = 0
                    while cnt < size:
                        line = stream.read(20)

                        # It's very clear in section 3.4.3 of the PDF spec
                        # that all cross-reference table lines are a fixed
                        # 20 bytes (as of PDF 1.7). However, some files have
                        # 21-byte entries (or more) due to the use of \r\n
                        # (CRLF) EOL's. Detect that case, and adjust the line
                        # until it does not begin with a \r (CR) or \n (LF).
                        while line[0] in b_("\x0D\x0A"):
                            stream.seek(-20 + 1, 1)
                            line = stream.read(20)

                        # On the other hand, some malformed PDF files
                        # use a single character EOL without a preceeding
                        # space.  Detect that case, and seek the stream
                        # back one character.  (0-9 means we've bled into
                        # the next xref entry, t means we've bled into the
                        # text "trailer"):
                        if line[-1] in b_("0123456789t"):
                            stream.seek(-1, 1)

                        offset, generation = line[:16].split(b_(" "))
                        offset, generation = int(offset), int(generation)
                        if generation not in self.xref:
                            self.xref[generation] = {}
                        if num in self.xref[generation]:
                            # It really seems like we should allow the last
                            # xref table in the file to override previous
                            # ones. Since we read the file backwards, assume
                            # any existing key is already set correctly.
                            pass
                        else:
                            self.xref[generation][num] = offset
                        cnt += 1
                        num += 1
                    readNonWhitespace(stream)
                    stream.seek(-1, 1)
                    trailertag = stream.read(7)
                    if trailertag != b_("trailer"):
                        # more xrefs!
                        stream.seek(-7, 1)
                    else:
                        break
                readNonWhitespace(stream)
                stream.seek(-1, 1)
                newTrailer = readObject(stream, self)
                for key, value in list(newTrailer.items()):
                    if key not in self.trailer:
                        self.trailer[key] = value
                if "/Prev" in newTrailer:
                    startxref = newTrailer["/Prev"]
                else:
                    break
            elif x.isdigit():
                # PDF 1.5+ Cross-Reference Stream
                stream.seek(-1, 1)
                idnum, generation = self.readObjectHeader(stream)
                xrefstream = readObject(stream, self)
                assert xrefstream["/Type"] == "/XRef"
                self.cacheIndirectObject(generation, idnum, xrefstream)
                streamData = BytesIO(b_(xrefstream.getData()))
                # Index pairs specify the subsections in the dictionary. If
                # none create one subsection that spans everything.
                idx_pairs = xrefstream.get("/Index", [0, xrefstream.get("/Size")])
                if debug: print(("read idx_pairs=%s"%list(self._pairs(idx_pairs))))
                entrySizes = xrefstream.get("/W")
                assert len(entrySizes) >= 3
                if self.strict and len(entrySizes) > 3:
                    raise utils.PdfReadError("Too many entry sizes: %s" %entrySizes)

                def getEntry(i):
                    # Reads the correct number of bytes for each entry. See the
                    # discussion of the W parameter in PDF spec table 17.
                    if entrySizes[i] > 0:
                        d = streamData.read(entrySizes[i])
                        return convertToInt(d, entrySizes[i])

                    # PDF Spec Table 17: A value of zero for an element in the
                    # W array indicates...the default value shall be used
                    if i == 0:  return 1 # First value defaults to 1
                    else:       return 0

                def used_before(num, generation):
                    # We move backwards through the xrefs, don't replace any.
                    return num in self.xref.get(generation, []) or \
                            num in self.xref_objStm

                # Iterate through each subsection
                last_end = 0
                for start, size in self._pairs(idx_pairs):
                    # The subsections must increase
                    assert start >= last_end
                    last_end = start + size
                    for num in range(start, start+size):
                        # The first entry is the type
                        xref_type = getEntry(0)
                        # The rest of the elements depend on the xref_type
                        if xref_type == 0:
                            # linked list of free objects
                            next_free_object = getEntry(1)
                            next_generation = getEntry(2)
                        elif xref_type == 1:
                            # objects that are in use but are not compressed
                            byte_offset = getEntry(1)
                            generation = getEntry(2)
                            if generation not in self.xref:
                                self.xref[generation] = {}
                            if not used_before(num, generation):
                                self.xref[generation][num] = byte_offset
                                if debug: print(("XREF Uncompressed: %s %s"%(
                                                num, generation)))
                        elif xref_type == 2:
                            # compressed objects
                            objstr_num = getEntry(1)
                            obstr_idx = getEntry(2)
                            generation = 0 # PDF spec table 18, generation is 0
                            if not used_before(num, generation):
                                if debug: print(("XREF Compressed: %s %s %s"%(
                                        num, objstr_num, obstr_idx)))
                                self.xref_objStm[num] = (objstr_num, obstr_idx)
                        elif self.strict:
                            raise utils.PdfReadError("Unknown xref type: %s"%
                                                        xref_type)

                trailerKeys = "/Root", "/Encrypt", "/Info", "/ID"
                for key in trailerKeys:
                    if key in xrefstream and key not in self.trailer:
                        self.trailer[NameObject(key)] = xrefstream.raw_get(key)
                if "/Prev" in xrefstream:
                    startxref = xrefstream["/Prev"]
                else:
                    break
            else:
                # bad xref character at startxref.  Let's see if we can find
                # the xref table nearby, as we've observed this error with an
                # off-by-one before.
                stream.seek(-11, 1)
                tmp = stream.read(20)
                xref_loc = tmp.find(b_("xref"))
                if xref_loc != -1:
                    startxref -= (10 - xref_loc)
                    continue
                # No explicit xref table, try finding a cross-reference stream.
                stream.seek(startxref, 0)
                found = False
                for look in range(5):
                    if stream.read(1).isdigit():
                        # This is not a standard PDF, consider adding a warning
                        startxref += look
                        found = True
                        break
                if found:
                    continue
                # no xref table found at specified location
                raise utils.PdfReadError("Could not find xref table at specified location")
        #if not zero-indexed, verify that the table is correct; change it if necessary
        if self.xrefIndex and not self.strict:
            loc = stream.tell()
            for gen in self.xref:
                if gen == 65535: continue
                for id in self.xref[gen]:
                    stream.seek(self.xref[gen][id], 0)
                    try:
                        pid, pgen = self.readObjectHeader(stream)
                    except ValueError:
                        break
                    if pid == id - self.xrefIndex:
                        self._zeroXref(gen)
                        break
                    #if not, then either it's just plain wrong, or the non-zero-index is actually correct
            stream.seek(loc, 0) #return to where it was

    def _zeroXref(self, generation):
        self.xref[generation] = dict( (k-self.xrefIndex, v) for (k, v) in list(self.xref[generation].items()) )

    def _pairs(self, array):
        i = 0
        while True:
            yield array[i], array[i+1]
            i += 2
            if (i+1) >= len(array):
                break

    def readNextEndLine(self, stream):
        debug = False
        if debug: print(">>readNextEndLine")
        line = b_("")
        while True:
            # Prevent infinite loops in malformed PDFs
            if stream.tell() == 0:
                raise utils.PdfReadError("Could not read malformed PDF file")
            x = stream.read(1)
            if debug: print(("  x:", x, "%x"%ord(x)))
            if stream.tell() < 2:
                raise utils.PdfReadError("EOL marker not found")
            stream.seek(-2, 1)
            if x == b_('\n') or x == b_('\r'): ## \n = LF; \r = CR
                crlf = False
                while x == b_('\n') or x == b_('\r'):
                    if debug:
                        if ord(x) == 0x0D: print("  x is CR 0D")
                        elif ord(x) == 0x0A: print("  x is LF 0A")
                    x = stream.read(1)
                    if x == b_('\n') or x == b_('\r'): # account for CR+LF
                        stream.seek(-1, 1)
                        crlf = True
                    if stream.tell() < 2:
                        raise utils.PdfReadError("EOL marker not found")
                    stream.seek(-2, 1)
                stream.seek(2 if crlf else 1, 1) #if using CR+LF, go back 2 bytes, else 1
                break
            else:
                if debug: print("  x is neither")
                line = x + line
                if debug: print(("  RNEL line:", line))
        if debug: print("leaving RNEL")
        return line
    
    def getIsEncrypted(self):
        return "/Encrypt" in self.trailer

    isEncrypted = property(lambda self: self.getIsEncrypted(), None, None)
    """
    Read-only boolean property showing whether this PDF file is encrypted.
    Note that this property, if true, will remain true even after the
    :meth:`decrypt()<PdfFileReader.decrypt>` method is called.
    """

class DocumentInformation(DictionaryObject):
    """
    A class representing the basic document metadata provided in a PDF File.
    This class is accessible through
    :meth:`getDocumentInfo()<PdfFileReader.getDocumentInfo()>`

    All text properties of the document metadata have
    *two* properties, eg. author and author_raw. The non-raw property will
    always return a ``TextStringObject``, making it ideal for a case where
    the metadata is being displayed. The raw property can sometimes return
    a ``ByteStringObject``, if pdf was unable to decode the string's
    text encoding; this requires additional safety in the caller and
    therefore is not as commonly accessed.
    """

    def __init__(self):
        DictionaryObject.__init__(self)

    def getText(self, key):
        retval = self.get(key, None)
        if isinstance(retval, TextStringObject):
            return retval
        return None

    title = property(lambda self: self.getText("/Title"))
    """Read-only property accessing the document's **title**.
    Returns a unicode string (``TextStringObject``) or ``None``
    if the title is not specified."""
    title_raw = property(lambda self: self.get("/Title"))
    """The "raw" version of title; can return a ``ByteStringObject``."""

    author = property(lambda self: self.getText("/Author"))
    """Read-only property accessing the document's **author**.
    Returns a unicode string (``TextStringObject``) or ``None``
    if the author is not specified."""
    author_raw = property(lambda self: self.get("/Author"))
    """The "raw" version of author; can return a ``ByteStringObject``."""

    subject = property(lambda self: self.getText("/Subject"))
    """Read-only property accessing the document's **subject**.
    Returns a unicode string (``TextStringObject``) or ``None``
    if the subject is not specified."""
    subject_raw = property(lambda self: self.get("/Subject"))
    """The "raw" version of subject; can return a ``ByteStringObject``."""

    creator = property(lambda self: self.getText("/Creator"))
    """Read-only property accessing the document's **creator**. If the
    document was converted to PDF from another format, this is the name of the
    application (e.g. OpenOffice) that created the original document from
    which it was converted. Returns a unicode string (``TextStringObject``)
    or ``None`` if the creator is not specified."""
    creator_raw = property(lambda self: self.get("/Creator"))
    """The "raw" version of creator; can return a ``ByteStringObject``."""

    producer = property(lambda self: self.getText("/Producer"))
    """Read-only property accessing the document's **producer**.
    If the document was converted to PDF from another format, this is
    the name of the application (for example, OSX Quartz) that converted
    it to PDF. Returns a unicode string (``TextStringObject``)
    or ``None`` if the producer is not specified."""
    producer_raw = property(lambda self: self.get("/Producer"))
    """The "raw" version of producer; can return a ``ByteStringObject``."""


def convertToInt(d, size):
    if size > 8:
        raise utils.PdfReadError("invalid size in convertToInt")
    d = b_("\x00\x00\x00\x00\x00\x00\x00\x00") + b_(d)
    d = d[-8:]
    return struct.unpack(">q", d)[0]

# ref: pdf1.8 spec section 3.5.2 algorithm 3.2
_encryption_padding = b_('\x28\xbf\x4e\x5e\x4e\x75\x8a\x41\x64\x00\x4e\x56') + \
        b_('\xff\xfa\x01\x08\x2e\x2e\x00\xb6\xd0\x68\x3e\x80\x2f\x0c') + \
        b_('\xa9\xfe\x64\x53\x69\x7a')


# Implementation of algorithm 3.2 of the PDF standard security handler,
# section 3.5.2 of the PDF 1.6 reference.
def _alg32(password, rev, keylen, owner_entry, p_entry, id1_entry, metadata_encrypt=True):
    # 1. Pad or truncate the password string to exactly 32 bytes.  If the
    # password string is more than 32 bytes long, use only its first 32 bytes;
    # if it is less than 32 bytes long, pad it by appending the required number
    # of additional bytes from the beginning of the padding string
    # (_encryption_padding).
    password = b_((str_(password) + str_(_encryption_padding))[:32])
    # 2. Initialize the MD5 hash function and pass the result of step 1 as
    # input to this function.
    import struct
    m = md5(password)
    # 3. Pass the value of the encryption dictionary's /O entry to the MD5 hash
    # function.
    m.update(owner_entry.original_bytes)
    # 4. Treat the value of the /P entry as an unsigned 4-byte integer and pass
    # these bytes to the MD5 hash function, low-order byte first.
    p_entry = struct.pack('<i', p_entry)
    m.update(p_entry)
    # 5. Pass the first element of the file's file identifier array to the MD5
    # hash function.
    m.update(id1_entry.original_bytes)
    # 6. (Revision 3 or greater) If document metadata is not being encrypted,
    # pass 4 bytes with the value 0xFFFFFFFF to the MD5 hash function.
    if rev >= 3 and not metadata_encrypt:
        m.update(b_("\xff\xff\xff\xff"))
    # 7. Finish the hash.
    md5_hash = m.digest()
    # 8. (Revision 3 or greater) Do the following 50 times: Take the output
    # from the previous MD5 hash and pass the first n bytes of the output as
    # input into a new MD5 hash, where n is the number of bytes of the
    # encryption key as defined by the value of the encryption dictionary's
    # /Length entry.
    if rev >= 3:
        for i in range(50):
            md5_hash = md5(md5_hash[:keylen]).digest()
    # 9. Set the encryption key to the first n bytes of the output from the
    # final MD5 hash, where n is always 5 for revision 2 but, for revision 3 or
    # greater, depends on the value of the encryption dictionary's /Length
    # entry.
    return md5_hash[:keylen]


# Implementation of algorithm 3.3 of the PDF standard security handler,
# section 3.5.2 of the PDF 1.6 reference.
def _alg33(owner_pwd, user_pwd, rev, keylen):
    # steps 1 - 4
    key = _alg33_1(owner_pwd, rev, keylen)
    # 5. Pad or truncate the user password string as described in step 1 of
    # algorithm 3.2.
    user_pwd = b_((user_pwd + str_(_encryption_padding))[:32])
    # 6. Encrypt the result of step 5, using an RC4 encryption function with
    # the encryption key obtained in step 4.
    val = utils.RC4_encrypt(key, user_pwd)
    # 7. (Revision 3 or greater) Do the following 19 times: Take the output
    # from the previous invocation of the RC4 function and pass it as input to
    # a new invocation of the function; use an encryption key generated by
    # taking each byte of the encryption key obtained in step 4 and performing
    # an XOR operation between that byte and the single-byte value of the
    # iteration counter (from 1 to 19).
    if rev >= 3:
        for i in range(1, 20):
            new_key = ''
            for l in range(len(key)):
                new_key += chr(ord_(key[l]) ^ i)
            val = utils.RC4_encrypt(new_key, val)
    # 8. Store the output from the final invocation of the RC4 as the value of
    # the /O entry in the encryption dictionary.
    return val


# Steps 1-4 of algorithm 3.3
def _alg33_1(password, rev, keylen):
    # 1. Pad or truncate the owner password string as described in step 1 of
    # algorithm 3.2.  If there is no owner password, use the user password
    # instead.
    password = b_((password + str_(_encryption_padding))[:32])
    # 2. Initialize the MD5 hash function and pass the result of step 1 as
    # input to this function.
    m = md5(password)
    # 3. (Revision 3 or greater) Do the following 50 times: Take the output
    # from the previous MD5 hash and pass it as input into a new MD5 hash.
    md5_hash = m.digest()
    if rev >= 3:
        for i in range(50):
            md5_hash = md5(md5_hash).digest()
    # 4. Create an RC4 encryption key using the first n bytes of the output
    # from the final MD5 hash, where n is always 5 for revision 2 but, for
    # revision 3 or greater, depends on the value of the encryption
    # dictionary's /Length entry.
    key = md5_hash[:keylen]
    return key


# Implementation of algorithm 3.4 of the PDF standard security handler,
# section 3.5.2 of the PDF 1.6 reference.
def _alg34(password, owner_entry, p_entry, id1_entry):
    # 1. Create an encryption key based on the user password string, as
    # described in algorithm 3.2.
    key = _alg32(password, 2, 5, owner_entry, p_entry, id1_entry)
    # 2. Encrypt the 32-byte padding string shown in step 1 of algorithm 3.2,
    # using an RC4 encryption function with the encryption key from the
    # preceding step.
    U = utils.RC4_encrypt(key, _encryption_padding)
    # 3. Store the result of step 2 as the value of the /U entry in the
    # encryption dictionary.
    return U, key


# Implementation of algorithm 3.4 of the PDF standard security handler,
# section 3.5.2 of the PDF 1.6 reference.
def _alg35(password, rev, keylen, owner_entry, p_entry, id1_entry, metadata_encrypt):
    # 1. Create an encryption key based on the user password string, as
    # described in Algorithm 3.2.
    key = _alg32(password, rev, keylen, owner_entry, p_entry, id1_entry)
    # 2. Initialize the MD5 hash function and pass the 32-byte padding string
    # shown in step 1 of Algorithm 3.2 as input to this function.
    m = md5()
    m.update(_encryption_padding)
    # 3. Pass the first element of the file's file identifier array (the value
    # of the ID entry in the document's trailer dictionary; see Table 3.13 on
    # page 73) to the hash function and finish the hash.  (See implementation
    # note 25 in Appendix H.)
    m.update(id1_entry.original_bytes)
    md5_hash = m.digest()
    # 4. Encrypt the 16-byte result of the hash, using an RC4 encryption
    # function with the encryption key from step 1.
    val = utils.RC4_encrypt(key, md5_hash)
    # 5. Do the following 19 times: Take the output from the previous
    # invocation of the RC4 function and pass it as input to a new invocation
    # of the function; use an encryption key generated by taking each byte of
    # the original encryption key (obtained in step 2) and performing an XOR
    # operation between that byte and the single-byte value of the iteration
    # counter (from 1 to 19).
    for i in range(1, 20):
        new_key = b_('')
        for l in range(len(key)):
            new_key += b_(chr(ord_(key[l]) ^ i))
        val = utils.RC4_encrypt(new_key, val)
    # 6. Append 16 bytes of arbitrary padding to the output from the final
    # invocation of the RC4 function and store the 32-byte result as the value
    # of the U entry in the encryption dictionary.
    # (implementator note: I don't know what "arbitrary padding" is supposed to
    # mean, so I have used null bytes.  This seems to match a few other
    # people's implementations)
    return val + (b_('\x00') * 16), key
