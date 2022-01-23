
using System;
using CommandLine.Attributes;
using CommandLine.Attributes.Advanced;
using convert.Enums;

namespace convert
{
    public class ConvertConfig
    {
        /// <summary>
        /// Type of action by files
        /// </summary>
        [RequiredArgument(0, "action", "what need do: convert or unconvert")]
        public ActionType ActionType { get; private set; }

        /// <summary>
        /// Work path
        /// </summary>
        [OptionalArgument("./data/", "src", "Source directory from witch will be convert or unconvert")]
        public string SrcPath { get; private set; } = "./data/";

        /// <summary>
        /// Desctionation path of convertaion
        /// </summary>
        [OptionalArgument("./out/", "dest", "Destination directory witch will be put result of action")]
        public string DestPath { get; private set; } = "./out/";

        [OptionalArgument("./out/", "json-path", "Dictionary where save json files")]
        public string JsonPath { get; set; } = "./out/";

        [OptionalArgument("en.ru.txt", "postfix", "Postfix for txt translated files")]
        public string PostfixTranslated { get; set; } = "en.ru.txt";
    }
}
