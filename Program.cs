using System;
using System.Collections.Generic;
using System.IO;

namespace mbox_Checkin_Extraction
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Type the path to the source txt doc: ");
            string path = Console.ReadLine();
            string[] lines = System.IO.File.ReadAllLines(path);
            List<string> output = new List<string>();
            List<string> outputTechGaps = new List<string>();

            bool isAttached = false;    //VAriable that determines if it must save the line or not (only for the body of the email)
            bool isOriginal = true;     //This variable determines if the message does not contain "Subject: Fwd: " or "Subject: Re: "
            string combineBodyLines = "";


            // FIRST PART, DELETE EVERYTHING BUT THE SUBJECT AND THE BODY, CONCATENATE THE BODY
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("Subject: Fwd: ") || lines[i].Contains("Subject: Re: ") || lines[i].Contains("Subject: RE: "))
                    isOriginal = false; 
                if (isOriginal)
                {
                     if (lines[i].Length > 2)
                    {                    
                        if (i > 2 && lines[i - 1].Length > 9)
                            if (lines[i - 1].Substring(0, 9) == "Subject: ")
                                continue;
                        if(lines[i].Length>=9)
                            if(lines[i].Substring(0, 9) == "Subject: " && lines[i+1].Contains("Content-Type: ") && lines[i].Contains("|"))
                            {
                                output.Add("<span>" + lines[i] + "\r\n");
                            }                    
                        else if (lines[i].Substring(0, 9) == "Subject: " && !lines[i+1].Contains("Content-Type: ") && i <= lines.Length && lines[i].Contains("|"))
                        {
                            output.Add("<span>" + lines[i] + lines[i+1] + "\r\n");
                        }
                        if (lines[i].Length >= 6 && lines[i].Substring(0, 6) == "<span>")
                            isAttached = true;
                    }
                    if(lines[i].Length < 1 && combineBodyLines != "")
                    {
                        output.Add(combineBodyLines + "\r\n\r\n");
                        isAttached = false;
                        combineBodyLines = "";
                    }                                                                                                         
                    if(isAttached)
                        combineBodyLines = combineBodyLines + lines[i].Substring(0, lines[i].Length-1);
                }
                if (lines[i].Length >= 9)
                    if (lines[i].Substring(0, 9) == "Subject: ")
                        isOriginal = true;
            }
            //SECOND PART, CLEAN THE DATA
            
            for(int i = 0; i< output.Count; i++)
            {
                output[i] = output[i].Replace("<br/>", "\r\n");
                output[i] = output[i].Replace("</td></tr>", "\r\n");
                output[i] = output[i].Replace("</tr>", "\r\n");
                output[i] = output[i].Replace("<ul><li><p>", "\r\n\n");
                output[i] = output[i].Replace("</td><td>", "|");
                output[i] = output[i].Replace("</td><td >", "|");
                output[i] = output[i].Replace("</th><th >", "|");
                output[i] = output[i].Replace("<td x", "|<");
                output[i] = output[i].Replace("</strong>", "");
                output[i] = output[i].Replace("<strong>", "");
                output[i] = output[i].Replace("<span>", "");
                output[i] = output[i].Replace("</span>", "");
                output[i] = output[i].Replace("<tr><td>", "");
                output[i] = output[i].Replace("<br/", "");
                output[i] = output[i].Replace("=0A", "");
                output[i] = output[i].Replace("&#x0D;", "");
                int startPosition = 0;
                int deletedPositions = 0;
                bool needToDelete = false;
                string deletedString = "";
                if (output[i].Length > 1)
                    for(int j = 0; j < output[i].Length; j++)
                    {                        
                        if(output[i][j-deletedPositions].ToString() == "<")
                        {
                            startPosition = j;
                            needToDelete = true;
                        }
                        if (output[i][j - deletedPositions].ToString() == ">")
                        {
                            deletedString = deletedString + output[i].Substring(startPosition, j - startPosition+1);
                            output[i] = output[i].Remove(startPosition, j- startPosition+1);
                            needToDelete = false;
                            j = 0;
                        }
                        if(j == output[i].Length - 1 && needToDelete)
                        {
                            deletedString = deletedString + output[i].Substring(startPosition, j - startPosition+1);
                            output[i] = output[i].Remove(startPosition, j - startPosition+1);
                            needToDelete = false;
                            j = 0;
                        }
                    }
            }

            for (int i = 0; i < output.Count; i++)
            {
                if (output[i].Length > 26 && !(output[i].Substring(0, 26) == "Subject: Employee Check-In" || (output[i].Length > 100 && output[i].Substring(0, 100).Contains("will be working with us"))))
                {
                    output.RemoveAt(i);
                    i = 0;
                }
            }



                // THIRD PART - ORGANIZE THE NEW TABLES

            string employeeName = "";
            string startDate = "";
            string project = "";
            string preCombined = "";
            string englishLevel = "";
            string preCombinedHeaders = "employeeName |" + 
                                        "project |" + 
                                        "startDate |" + 
                                        "English Level |" +
                                        "Technology|" +
                                        "Experience - Validation Date|" +
                                        "NO Experience|" +
                                        "Experience (years)|" +
                                        "Experience - Validated by|" +
                                        "Experience - Validation comments|" +
                                        "Last date he/she worked with the tecnology|" +
                                        "Experience Type|Interest in technology|" +
                                        "Applicant auto-evaluation (in technology)|" +
                                        "BairesDev evaluation (in technology)|" +
                                        "BairesDev evaluation date|" +
                                        "BairesDev evaluation user|" +
                                        "BairesDev evaluation comments";
            outputTechGaps.Add(preCombinedHeaders);
            bool reset = false;
            for (int i = 0; i < output.Count; i++)
            {
                if(output[i].Length > 12)
                {                    
                    if(output[i].Substring(0,9) == "Subject: ")
                    {
                        employeeName = output[i].Substring(output[i].IndexOf("|"),output[i].Length - output[i].IndexOf("|"));
                        employeeName = employeeName.Remove(0,1);                        
                        employeeName = employeeName.Substring(employeeName.IndexOf("|"), employeeName.Length - employeeName.IndexOf("|"));
                        employeeName = employeeName.Remove(0, 2);
                        employeeName = employeeName.Replace("\r\n", "");
                        startDate = "";
                        project = "";
                        preCombined = "";
                        englishLevel = "";
                        reset = true;
                    }
                    if (output[i].Substring(0, 9) != "Subject: " && reset)
                    {
                        int indexDate = output[i].IndexOf("\r\n\r\nStart Date|") + "\r\n\r\nStart Date|".Length;
                        startDate = output[i].Substring(indexDate, 11);
                        int indexProjectStart = output[i].IndexOf("\r\nProject Name:|") + "\r\nProject Name:|".Length;
                        int indexProjectEnd = output[i].IndexOf("\r\nEmployee Id:|");
                        project = output[i].Substring(indexProjectStart, indexProjectEnd-indexProjectStart);                        
                        int startingPoint = output[i].IndexOf("BairesDev evaluation comments\r\n") + "BairesDev evaluation comments\r\n".Length;
                        int endingPoint = output[i].IndexOf("\r\n",startingPoint);
                        int finalPoint = output[i].IndexOf("Items to Check");
                        int startEnglishPoint = output[i].IndexOf("\r\nEnglish level:|") + "\r\nEnglish level:|".Length;
                        int endEnglishPoint = output[i].IndexOf("\r\nPersonal email:");
                        englishLevel = output[i].Substring(startEnglishPoint, endEnglishPoint - startEnglishPoint);
                        preCombined = employeeName + "|" + project + "|" + startDate + "|" + englishLevel + "|";
                        while (true)
                        {
                            if(output[i].Substring(startingPoint, endingPoint - startingPoint).Length>5)
                                outputTechGaps.Add(preCombined + output[i].Substring(startingPoint, endingPoint - startingPoint));
                            startingPoint = endingPoint + 3;
                            endingPoint = output[i].IndexOf("\r\n", startingPoint);
                            if (endingPoint > finalPoint)
                                break;
                        }
                        reset = false;
                    }

                }                
            }
            

            // Getting the output
            System.Text.StringBuilder ultimateOutput = new System.Text.StringBuilder();
            foreach (var item in outputTechGaps)
            {
                ultimateOutput.AppendLine(item.ToString());
            }
            System.IO.File.WriteAllText(
                System.IO.Path.Combine("C:/Users/nikil/Desktop/EmployeeCheckin/" + "output.txt"),ultimateOutput.ToString());
        }
    }
}
