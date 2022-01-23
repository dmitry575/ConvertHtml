using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace convert
{
    public class HtmlUnConvert : BaseHtmlConvert
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

        private readonly string _resultExtFile = ".html";

        public HtmlUnConvert(string path, string destPath, string jsonPath)
        {
            _srcPath = path;
            _destPath = destPath;
            _jsonPath = jsonPath;
            CreatePath(_destPath);
            CreatePath(_jsonPath);
        }

        /// <summary>
        /// Convert data from our format to html
        /// </summary>
        /// <param name="postfixFileData"></param>
        /// <param name="postfixFileTxt"></param>
        public void UnConvert(string postfixFileData, string postfixFileTxt)
        {
            string[] fileEntries = Directory.GetFiles(_srcPath, "*" + postfixFileTxt);
            int success = 0;
            int failed = 0;
            foreach (var fileEntry in fileEntries)
            {
                try
                {
                    Console.WriteLine($"read all from file: {fileEntry}");
                    var translate = File.ReadAllText(fileEntry);

                    var tags = GetTagsFromJson(fileEntry, postfixFileData);
                    if (tags == null)
                    {
                        failed++;
                        continue;
                    }


                    var html = GetUnClean(
                        GetUnGroup(GetAfterTranslate(translate), tags.Groups),
                        tags.Tags);

                    // save html
                    SaveDstResult(fileEntry, html);
                    success++;
                }
                catch (Exception e)
                {
                    failed++;
                    Console.WriteLine($"Error un convert file: {fileEntry}, {e}");
                }
            }

            Console.WriteLine($"Un convert {fileEntries.Length} files, success: {success}, failed: {failed}");
        }

        /// <summary>
        /// Save html result to dst
        /// </summary>
        /// <param name="fileEntry"></param>
        /// <param name="html"></param>
        private bool SaveDstResult(string fileEntry, string html)
        {
            var fInfo = new FileInfo(fileEntry);
            var htmlFilename = fInfo.Name.Substring(0, fInfo.Name.IndexOf("."));
            htmlFilename += (htmlFilename[htmlFilename.Length - 1] == '_' ? "" : "_") + _resultExtFile;

            WriteToFile(Path.Combine(_destPath, htmlFilename), html);

            Console.WriteLine($"file saved: {Path.Combine(_destPath, htmlFilename)}");
            return true;
        }

        /// <summary>
        /// Read information about tags from file
        /// </summary>
        /// <param name="fileEntry">base file name</param>
        /// <param name="postfixFileData">Postfix for json file</param>
        private JsonData GetTagsFromJson(string fileEntry, string postfixFileData)
        {
            var fInfo = new FileInfo(fileEntry);
            var jsonFilename = fInfo.Name.Substring(0, fInfo.Name.IndexOf("."));
            jsonFilename += postfixFileData;

            jsonFilename = Path.Combine(_jsonPath, jsonFilename);

            if (!File.Exists(jsonFilename))
                return null;

            Console.WriteLine($"read from json file: {jsonFilename}");

            return JsonConvert.DeserializeObject<JsonData>(File.ReadAllText(jsonFilename));
        }

        private string GetUnClean(string translate, Dictionary<int, string> tags)
        {
            var result = translate;
            foreach (var tag in tags)
            {
                var key = $"{PrefixTag}{tag.Key}";

                Regex regexTag = new Regex(@"\[\s*(" + key + @")\s*\]");
                if (regexTag.IsMatch(result))
                {
                    result = regexTag.Replace(result, "[$1]");
                }


                if (!result.Contains("[" + key + "]"))
                {
                    throw new Exception($"not found key {key} for {tag.Value}");
                }
                result = result.Replace("[" + key + "]", tag.Value);
            }

            return result;
        }

        private string GetUnGroup(string translate, Dictionary<int, string> tagsGroups)
        {
            var result = translate;
            foreach (var tagsGroup in tagsGroups)
            {
                var key = $"{GroupPrefixTag}{tagsGroup.Key}";

                Regex regexTag = new Regex(@"\[\s*(" + key + @")\s*\]");
                if (regexTag.IsMatch(result))
                {
                    result = regexTag.Replace(result, "[$1]");
                }

                if (!result.Contains("[" + key + "]"))
                {
                    throw new Exception($"not found key [{key}] for {tagsGroup.Value}");
                }

                result = result.Replace("[" + key + "]", tagsGroup.Value);
            }

            return result;
        }

        public string GetAfterTranslate(string translate)
        {
            string clean = translate;
            Regex regex = new Regex(@"\[\s*(" + PrefixTag + @"[0-9]+)\s*\]");
            if (regex.IsMatch(clean))
            {
                clean = regex.Replace(clean, "[$1]");
            }
            Regex regexGroup = new Regex(@"\[\s*(" + GroupPrefixTag + @"[0-9]+)\s*\]");
            if (regexGroup.IsMatch(clean))
            {
                clean = regexGroup.Replace(clean, "[$1]");
            }
            Regex regexComment = new Regex(@"[\-]+(?<space>\s)*[\-]*(?<space>\s)*121(?<space>\s)*(?<space>\.)*(?<space>\s)*[\-]+(?<space>\s)*[\-]+(?<space>\s)*[\-]*(?<space>\s)*[\d]+(?<space>\s)*[\-]+");
            if (regexComment.IsMatch(clean))
            {
                clean = regexComment.Replace(clean, ProcessSpace);
            }
            Regex regexpoint = new Regex(@"[\-]+121[\-]+[\d]+(\.)");
            if (regexpoint.IsMatch(clean))
            {
                clean = regexpoint.Replace(clean, ProcessPoint);
            }

            return clean;
        }

        /// <summary>
        /// Clear double spaces
        /// </summary>
        private static string ProcessSpace(Match m)
        {

            return m.Value.Replace(" ", "").Replace(".", "");
        }

        /// <summary>
        /// Clear double spaces
        /// </summary>
        private static string ProcessPoint(Match m)
        {

            return m.Value.Replace(".", "-");
        }

    }
}
