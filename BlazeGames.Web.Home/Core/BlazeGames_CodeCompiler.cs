using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BlazeGames.Web.Core;

namespace BlazeGames.Web
{
    public class BlazeGames_CodeCompiler
    {
        string[] Methods = new string[] { "@exit", "@print" };
        

        public BlazeGames_CodeCompiler()
        {

        }

        public string CompileToCSharp(string InputCode)
        {
            string OutputCode = InputCode;

            string[][] Prints = GetStringInBetween("@print:", ";", OutputCode);

            List<string[]> ExitsList = new List<string[]>();
            ExitsList.AddRange(GetStringInBetween("@exit:", ";", OutputCode));
            ExitsList.AddRange(GetStringInBetween("@exit", ";", OutputCode));
            string[][] Exits = ExitsList.ToArray();

            string[][] Ifs = GetStringInBetween("@if:", ";", OutputCode);
            List<string[]> ElsesList = new List<string[]>();
            ElsesList.AddRange(GetStringInBetween("@else", ";", OutputCode));
            ElsesList.AddRange(GetStringInBetween("@else:", ";", OutputCode));
            string[][] Elses = ElsesList.ToArray();
            string[][] Ends = GetStringInBetween("@end", ";", OutputCode);

            string[][] POSTs = GetStringInBetween("@post:", ";", OutputCode);
            string[][] GETs = GetStringInBetween("@get:", ";", OutputCode);

            if (Prints != null)
            {
                foreach (string[] codeinfo in Prints)
                {
                    OutputCode = OutputCode.Replace(codeinfo[0], "Http.Response.Write(" + codeinfo[1] + ");");
                }
            }

            
            if (Exits != null)
            {
                foreach (string[] codeinfo in Exits)
                {
                    if (codeinfo[1] == "")
                        OutputCode = OutputCode.Replace(codeinfo[0], "Http.Response.End();");
                    else
                        OutputCode = OutputCode.Replace(codeinfo[0], "Http.Response.Write(" + codeinfo[1] + "); Http.Response.End();");
                }
            }

            if (Ifs != null)
            {
                foreach (string[] codeinfo in Ifs)
                {
                    OutputCode = OutputCode.Replace(codeinfo[0], "if(" + codeinfo[1] + ") {");
                }
            }

            if (Elses != null)
            {
                foreach (string[] codeinfo in Elses)
                {
                    if (codeinfo[1] == "")
                        OutputCode = OutputCode.Replace(codeinfo[0], "} else {");
                    else
                        OutputCode = OutputCode.Replace(codeinfo[0], "} else if(" + codeinfo[1] + ") {");
                }
            }

            if (Ends != null)
            {
                foreach (string[] codeinfo in Ends)
                {
                    OutputCode = OutputCode.Replace(codeinfo[0], "}");
                }
            }

            if (POSTs != null)
            {
                foreach (string[] codeinfo in POSTs)
                {
                    OutputCode = OutputCode.Replace(codeinfo[0], "Utilities.POST(\"" + codeinfo[1] + "\")");
                }
            }

            if (GETs != null)
            {
                foreach (string[] codeinfo in GETs)
                {
                    OutputCode = OutputCode.Replace(codeinfo[0], "Utilities.GET(\"" + codeinfo[1] + "\")");
                }
            }

            //HttpContext.Current.Response.Write(OutputCode);
            //HttpContext.Current.Response.End();

            return OutputCode;
        }

        private string[][] GetStringInBetween(string strBegin, string strEnd, string strSource)
        {
            int current_index = 0;
            List<string[]> result = new List<string[]>();

            //string[] result = { "", "" };

            while(true)
            {
                int iIndexOfBegin = strSource.IndexOf(strBegin, current_index);
                if (iIndexOfBegin != -1)
                {
                    iIndexOfBegin -= strBegin.Length;

                    strSource = strSource.Substring(iIndexOfBegin
                        + strBegin.Length);
                    int iEnd = strSource.IndexOf(strEnd);
                    if (iEnd != -1)
                    {
                        iEnd += strEnd.Length;

                        string str = strSource.Substring(0, iEnd);

                        result.Add(new string[] {
                            str,
                            str.Replace(strBegin, "").Replace(strEnd, "")
                        });

                        current_index = iEnd;
                    }
                    else
                        return null;

                }
                else
                    break;
            }

            return result.ToArray();
        }
    }
}