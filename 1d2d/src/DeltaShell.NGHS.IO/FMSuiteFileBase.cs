namespace DeltaShell.NGHS.IO
{
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
            var i = 0;
            var contentIdentifier = new char[line.Length];
            foreach (var c in line)
            {
                if (c == ' ' || c == '\t')
                    continue;

                if (!fileIsExtForcesFile && c == '=')
                    break;

                if (c == '#' || c == '!' || c == '*')
                    break;

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
                        var contentIdentifier = CreateContentIdentifier(CurrentLine.Trim() + storedNextInputLine.Trim());
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
                        var contentIdentifier = CreateContentIdentifier(storedNextOutputLine + line.Trim());
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
