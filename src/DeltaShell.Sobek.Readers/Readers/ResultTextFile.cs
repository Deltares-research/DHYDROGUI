using System.Collections.Generic;
using System.IO;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class ResultTextFile
    {
        private StreamReader reader;
        private StreamWriter writer;
        private string currentLine;
        private List<string> comments = new List<string>();
        private List<int> commentBlocks = new List<int>();

        private string path;
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        private int currentLineIndex;
        public int CurrentLineIndex
        {
            get { return currentLineIndex; }
            set { currentLineIndex = value; }
        }
        private int currentCommentIndex;

        public void Open(string path, bool write)
        {
            currentLineIndex = 0;
            currentCommentIndex = 0;
            if (write)
            {
                reader = null;
                writer = new StreamWriter(path);
            }
            else
            {
                reader = File.OpenText(path);
                writer = null;
                commentBlocks = new List<int>();
                commentBlocks.Add(0);
            }
        }

        public bool IsEmpty()
        {
            if (reader != null)
            {
                return reader.Peek() == -1;
            }
            
            throw new FileLoadException("File is not open");
        }

        public void Close()
        {
            if (null != reader)
                reader.Close();
            if (null != writer)
                writer.Close();
        }
        private string ReadLine()
        {
            currentLineIndex++;
            return reader.ReadLine();
        }
        public void WriteLine(string line, bool pushComments)
        {
            if (pushComments)
            {
                for (int i = commentBlocks[currentCommentIndex]; i < commentBlocks[currentCommentIndex + 1]; i++)
                {
                    writer.WriteLine(comments[i]);
                }
                currentCommentIndex++;
            }
            writer.WriteLine(line);
        }
        private void SkipComments()
        {
            while (currentLine != null && currentLine.StartsWith("*"))
            {
                currentCommentIndex++;
                comments.Add(currentLine);
                currentLine = ReadLine();
            }
        }
        public string ParseLine(bool popComments)
        {
            currentLine = this.ReadLine();
            SkipComments();
            if (popComments)
                PopComments();
            return currentLine;
        }
        public void PopComments()
        {
            commentBlocks.Add(currentCommentIndex);
        }

    }
}