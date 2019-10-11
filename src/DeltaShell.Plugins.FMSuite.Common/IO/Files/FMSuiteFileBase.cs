using DeltaShell.NGHS.IO;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    // TODO YAGNI: storing comment blocks with unique keys is too ambitious, just queue them and let inheritants make a dictionary, 
    // TODO: remove the extforcefile logic from this class

    public class FMSuiteFileBase : NGHSFileBase
    {
        private const string ExtForcesFileQuantBlockStarter = "QUANTITY=";
        private readonly bool fileIsExtForcesFile;

        public FMSuiteFileBase(bool fileIsExtForcesFile = false)
        {
            this.fileIsExtForcesFile = fileIsExtForcesFile;
        }

        protected override string CreateContentIdentifier(string line)
        {
            if (line == null)
            {
                return string.Empty;
            }

            var i = 0;
            var contentIdentifier = new char[line.Length];
            foreach (char c in line)
            {
                if (c == ' ' || c == '\t')
                {
                    continue;
                }

                if (!fileIsExtForcesFile && c == '=')
                {
                    break;
                }

                if (c == '#' || c == '!' || c == '*')
                {
                    break;
                }

                contentIdentifier[i++] = c;
            }

            return new string(contentIdentifier, 0, i);
        }

        protected override void CreateCommonBlock()
        {
            if (fileIsExtForcesFile)
            {
                if (CurrentLine.ToUpper().StartsWith(ExtForcesFileQuantBlockStarter))
                {
                    LineNumber++;
                    storedNextInputLine = reader.ReadLine();
                    if (storedNextInputLine != null)
                    {
                        string contentIdentifier =
                            CreateContentIdentifier(CurrentLine.Trim() + storedNextInputLine.Trim());
                        commentBlocks.Add(contentIdentifier, currentCommentBlock);
                    }
                }
                else
                {
                    // can not handle internal comments
                    currentCommentBlock = null;
                }
            }
            else
            {
                base.CreateCommonBlock();
            }
        }

        protected override bool WriteCommentBlock(string line, bool doWriteLine)
        {
            if (fileIsExtForcesFile)
            {
                if (line.ToUpper().StartsWith(ExtForcesFileQuantBlockStarter))
                {
                    storedNextOutputLine = line;
                    doWriteLine = false;
                }
                else
                {
                    if (storedNextOutputLine != null)
                    {
                        string contentIdentifier = CreateContentIdentifier(storedNextOutputLine + line.Trim());
                        if (commentBlocks.ContainsKey(contentIdentifier))
                        {
                            foreach (string commentLine in commentBlocks[contentIdentifier])
                            {
                                writer.WriteLine(commentLine);
                            }
                        }

                        writer.WriteLine(storedNextOutputLine);
                        storedNextOutputLine = null;
                    }
                }
            }
            else
            {
                doWriteLine = base.WriteCommentBlock(line, doWriteLine);
            }

            return doWriteLine;
        }
    }
}