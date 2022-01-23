using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace convert
{

    public class HtmlConvert : BaseHtmlConvert
    {
        /// <summary>
        /// source path
        /// </summary>
        private readonly string _srcPath;

        /// <summary>
        /// Destination path for saving results
        /// </summary>
        private readonly string _destPath;

        /// <summary>
        /// Path with jsons files, need for unconvert data
        /// </summary>
        private readonly string _jsonPath;

        private readonly Dictionary<int, string> _htmlTags = new Dictionary<int, string>();

        private readonly Dictionary<int, string> _groupTags = new Dictionary<int, string>();

        private readonly List<string> _tagsNotTranslate = new List<string> { "pre", "code" };

        private readonly char[] _listNumbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private readonly char[] _listSpaces = { ' ', '\r', '\n' };

        private int _index = 1;

        private readonly Regex _regexSpace = new Regex("[ ]{2,}", RegexOptions.None);

        public HtmlConvert(string srcPath, string destPath, string jsonPath)
        {
            _srcPath = srcPath;
            _destPath = destPath;
            _jsonPath = jsonPath;
            CreatePath(_srcPath);
            CreatePath(_destPath);
            CreatePath(_jsonPath);
        }

        /// <summary>
        /// Convert data from tags to format with number
        /// </summary>
        /// <param name="mapFiles"></param>
        /// <param name="postfixFileData"></param>
        /// <param name="postfixFileTxt"></param>
        public void Convert(string mapFiles, string postfixFileData = "_data.json", string postfixFileTxt = "_.txt")
        {

            // get all files from path
            foreach (var fileEntry in Directory.GetFiles(_srcPath, mapFiles))
            {
                try
                {
                    // get all content from file
                    var html = File.ReadAllText(fileEntry);

                    // clear html
                    var clean = PipelineClean(html);

                    // save clean text to file
                    SaveTxtFile(fileEntry, postfixFileTxt, clean);

                    // save data for unconvert
                    SaveJsonFile(fileEntry, postfixFileData);

                    _htmlTags.Clear();
                    _groupTags.Clear();
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
            }
        }

        /// <summary>
        /// Save for text file
        /// </summary>
        /// <param name="fileBase">Source filename</param>
        /// <param name="postfixFileTxt">Postfix of text file or result file</param>
        /// <param name="clean">Clean information for saving</param>
        private void SaveTxtFile(string fileBase, string postfixFileTxt, string clean)
        {
            var fInfo = new FileInfo(fileBase);

            var newFileClean = fInfo.Name.Substring(0, fInfo.Name.IndexOf("."));
            newFileClean += postfixFileTxt;
            WriteToFile(Path.Combine(_destPath, newFileClean), clean);
            Console.WriteLine($"txt file saved: {newFileClean}");
        }

        /// <summary>
        /// Save data to different files
        /// </summary>
        /// <param name="fileBase">Source filename</param>
        /// <param name="postfixFileData">Postfix of data file</param>
        private void SaveJsonFile(string fileBase, string postfixFileData)
        {
            var fInfo = new FileInfo(fileBase);
            var newFile = fInfo.Name.Substring(0, fInfo.Name.IndexOf("."));
            newFile += postfixFileData;

            WriteToFile(Path.Combine(_jsonPath, newFile), new JsonData { Tags = _htmlTags, Groups = _groupTags });
            Console.WriteLine($"json file saved: {newFile}");
        }


        /// <summary>
        /// Pipeline to clear html
        /// </summary>
        /// <param name="html">Dirty html with tags</param>
        private string PipelineClean(string html)
        {
            var clean = GetClean(html);

            clean = GetGroup(clean);

            return WebUtility.HtmlDecode(clean);
        }

        /// <summary>
        /// Grouping new data
        /// </summary>
        /// <param name="clean"></param>
        private string GetGroup(string clean)
        {
            clean = clean.Trim();
            clean = clean.Replace("\r\n", " ");

            // empty or space symbol
            const int EMPTY = 0;

            // current position int any tags
            const int INSERT_TAG = 1;

            // current position after close tag
            const int AFTER_TAG_EMPTY = 2;

            int status = 0;
            int cur = 0;
            int count = 0;
            int index = 0;

            for (int i = 0; i < clean.Length; i++)
            {
                switch (status)
                {
                    case EMPTY:
                        if (clean[i] == '[')
                        {
                            if (clean.Substring(i + 1, 2) == PrefixTag)
                            {
                                cur = i;
                                status = INSERT_TAG;
                                count++;
                            }
                        }

                        break;

                    case INSERT_TAG:
                        if (clean[i] == ']')
                        {
                            status = AFTER_TAG_EMPTY;
                            break;
                        }

                        if (!_listNumbers.Contains(clean[i]))
                        {
                            count = 0;
                            status = EMPTY;
                        }


                        break;

                    case AFTER_TAG_EMPTY:
                        if (clean[i] == '[')
                        {
                            status = INSERT_TAG;
                            count++;
                            break;
                        }

                        if (!_listSpaces.Contains(clean[i]))
                        {
                            if (count > 1)
                            {
                                _groupTags.Add(index++, clean.Substring(cur, i - cur));
                            }

                            count = 0;
                            status = EMPTY;
                        }
                        break;
                }

            }

            if (count > 0 && status == AFTER_TAG_EMPTY && cur > 0)
            {
                _groupTags.Add(index, clean.Substring(cur, clean.Length - cur));
            }

            foreach (var key in _groupTags.Keys.ToArray())
            {
                clean = clean.Replace(_groupTags[key], $" [{GroupPrefixTag}{key}] ");
                _groupTags[key] = _regexSpace.Replace(_groupTags[key], " ");
            }

            clean = clean.Replace(".[11", ". [11");
            clean = clean.Replace("![11", "! [11");
            clean = clean.Replace(".[12", ". [12");
            clean = clean.Replace("![12", "! [12");

            clean = _regexSpace.Replace(clean, " ");
            return clean;
        }


        /// <summary>
        /// Clean html remove html tags
        /// </summary>
        /// <param name="html">Dirty html</param>
        private string GetClean(string html)
        {
            HtmlDocument hap = new HtmlDocument();
            try
            {
                hap.OptionWriteEmptyNodes = true;
                hap.LoadHtml(html);
                html = CleanHtml(hap);

                return html;

            }
            catch (Exception e)
            {
                Console.Write(e);
            }

            return string.Empty;
        }

        /// <summary>
        /// Clearing Html
        /// </summary>
        /// <param name="hap"></param>
        private string CleanHtml(HtmlDocument hap)
        {
            var nodes = new Queue<HtmlNode>(hap.DocumentNode.SelectNodes("./*|./text()"));
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();

                var parentNode = node.ParentNode;
                if (_tagsNotTranslate.Contains(node.Name) && node.Name != "#text")
                {
                    _htmlTags.Add(_index, node.OuterHtml);
                    HtmlNode child = HtmlNode.CreateNode($"[{PrefixTag}{_index}]");
                    _index++;

                    parentNode.InsertBefore(child, node);
                    parentNode.RemoveChild(node);
                }
            }

            nodes = new Queue<HtmlNode>(hap.DocumentNode.SelectNodes("./*|./text()"));
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                if (node.Name != "#text")
                {

                    _htmlTags.Add(_index, GetTagAttributs(node));
                    HtmlNode newNode = HtmlNode.CreateNode($"[{PrefixTag}{_index}]");
                    _index++;

                    parentNode.InsertBefore(newNode, node);

                    // closing tag
                    if (!HtmlNode.IsEmptyElement(node.Name))
                    {
                        _htmlTags.Add(_index, GetTagClose(node));
                        HtmlNode newNodeClose = HtmlNode.CreateNode($"[{PrefixTag}{_index}]");
                        _index++;
                        parentNode.InsertAfter(newNodeClose, node);
                    }
                    var childNodes = node.SelectNodes("./*|./text()");

                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            nodes.Enqueue(child);
                            parentNode.InsertBefore(child, node);
                        }
                    }

                    parentNode.RemoveChild(node);
                }
            }

            return hap.DocumentNode.InnerHtml;
        }

        private string GetTagAttributs(HtmlNode node)
        {
            var sb = new StringBuilder();
            sb.Append("<");
            sb.Append(node.Name);
            foreach (var attribute in node.Attributes)
            {
                string str1 = attribute.QuoteType == AttributeValueQuote.DoubleQuote ? "\"" : "'";
                sb.Append(" " + attribute.OriginalName + "=" + str1 + HtmlDocument.HtmlEncode(attribute.Value) + str1);
            }

            if (HtmlNode.IsEmptyElement(node.Name))
            {
                sb.Append(" /");
            }
            sb.Append(">");

            return sb.ToString();
        }

        /// <summary>
        /// Get close tag
        /// </summary>
        private string GetTagClose(HtmlNode node)
        {
            return string.Format("</" + node.OriginalName + ">");
        }


    }
}