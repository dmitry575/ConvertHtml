using System;
using CommandLine;
using convert.Enums;

namespace convert
{
    static class Program
    {
        private const string Version = "1.07";
        private const string GitHub = "https://github.com/dmitry575";
        static void Main(string[] args)
        {
            Console.WriteLine($"Convert html file to another format. Version: {Version}");
            Console.WriteLine($"Url to github: {GitHub}");
            Console.WriteLine("");
            if (!Parser.TryParse(args, out ConvertConfig config))
            {
                Parser.DisplayHelp<ConvertConfig>(HelpFormat.Full);
                return;
            }

            if (config.ActionType == ActionType.convert)
            {
                new HtmlConvert(config.SrcPath, config.DestPath, config.JsonPath)
                    .Convert("*.html");
            }

            if (config.ActionType == ActionType.unconvert)
            {
                try
                {
                    new HtmlUnConvert(config.SrcPath, config.DestPath, config.JsonPath)
                        .UnConvert("data.json", "." + config.PostfixTranslated);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error un convert files: src: {config.SrcPath}, dest: {config.DestPath}, {e}");
                }
            }
        }
    }
}
