using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleFileMover
{
    class Program
    {
        public static Configuration cif = new Configuration();
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            cif.loadXML("C:\\Users\\Jan\\source\\repos\\SimpleFileMover\\SimpleFileMover.config.xml");

            for (int i = 0; i < cif.tasks.Length; i++) {
                cif.tasks[i].fullaction();

            }

            Console.ReadKey();
        }

        public void taskController() {


        }

    }

    class Configuration
    {

        public Task[] tasks;
        public BackupController backupController;
        public TimeTableEngine timeTableEngine;
        //read for new Configuration File
        string fullName = "C:\\Users\\Jan\\source\\repos\\SimpleFileMover\\SimpleFileMover.config.xml";
        XmlDocument xreader = new XmlDocument();

        public void loadXML(String path) {
            xreader.Load(path);
            Console.WriteLine("test if writing");
            Console.WriteLine(xreader.SelectSingleNode("config/tasks/task/@id").Value);
            XmlNodeList tasknodes = xreader.SelectNodes("config/tasks/task");
            tasks = new Task[tasknodes.Count];
            int taskIterator = 0;
            foreach (XmlNode tasknode in tasknodes) {
                Task task = new Task();

                //TaskSpecific
                XmlNode id = tasknode.SelectSingleNode("@id");
                XmlNode enableTask = tasknode.SelectSingleNode("@enable");
                task.taskid = id.Value;
                task.taskEnable = parseBooleanFromString(enableTask.Value);
                if ((id != null) && (enableTask != null)) {

                    Console.WriteLine("task: " + id.Value + " is " + enableTask.Value);

                }
                //Task timer node
                XmlNode timerNode = tasknode.SelectSingleNode("timer");
                XmlNode timerIteration = timerNode.SelectSingleNode("@iteration");
                XmlNode timeTableRef = timerNode.SelectSingleNode("@timeTableRef");
                task.timeTableRef = timeTableRef.Value;
                if (timerNode == null)
                {
                    //Standart Timing should be 30 sec
                    task.milsecIteration = parseTimeString2Mil("30s");
                    Console.WriteLine(parseTimeString2Mil("30s") + " ::is parsed Milsec");
                }
                else {
                    //parse Timing somwhere
                    task.milsecIteration = parseTimeString2Mil(timerIteration.Value);
                    Console.WriteLine(parseTimeString2Mil(timerIteration.Value) + " ::is parsed Milsec");
                }
                Console.WriteLine("Timer Set to: " + timerIteration.Value + " using TimeTable: " + timeTableRef.Value);

                //task Input Node
                XmlNode inputNode = tasknode.SelectSingleNode("input");
                XmlNode inputPath = inputNode.SelectSingleNode("@path");
                XmlNode inputMask = inputNode.SelectSingleNode("@mask");
                XmlNode inputDepth = inputNode.SelectSingleNode("@depth");
                XmlNode inputRecursive = inputNode.SelectSingleNode("@recursive");
                task.inputPath = inputPath.Value;
                task.inputMask = inputMask.Value;
                task.inputDepth = int.Parse(inputDepth.Value);
                task.recusive = parseBooleanFromString(inputRecursive.Value);

                //task Temp node
                XmlNode tempNode = tasknode.SelectSingleNode("temp");
                XmlNode tempEnable = tempNode.SelectSingleNode("@enable");
                XmlNode tempPath = tempNode.SelectSingleNode("@path");
                XmlNode tempMask = tempNode.SelectSingleNode("@mask");
                task.enableTemporary = parseBooleanFromString(tempEnable.Value);
                task.tempPath = tempPath.Value;
                task.tempMask = tempMask.Value;

                //task rename by Date
                XmlNode renameByDateNode = tasknode.SelectSingleNode("renameByDate");
                XmlNode renameByDateEnable = renameByDateNode.SelectSingleNode("@enable");
                XmlNode renameByDatePrefix = renameByDateNode.SelectSingleNode("@prefix");
                XmlNode renameByDateName = renameByDateNode.SelectSingleNode("@name");
                XmlNode renameByDateSuffix = renameByDateNode.SelectSingleNode("@suffix");
                task.enableRenameByDate = parseBooleanFromString(renameByDateEnable.Value);
                task.renameByDatePrefix = renameByDatePrefix.Value;
                task.renameByDateNameCondition = renameByDateName.Value;
                task.renameByDateSuffix = renameByDateSuffix.Value;

                //task rename by Count
                XmlNode renameByCountNode = tasknode.SelectSingleNode("renameByCount");
                XmlNode renameByCountEnable = renameByCountNode.SelectSingleNode("@enable");
                XmlNode renameByCountPrefix = renameByCountNode.SelectSingleNode("@prefix");
                XmlNode renameByCountSuffix = renameByCountNode.SelectSingleNode("@suffix");
                XmlNode renameByCountStart = renameByCountNode.SelectSingleNode("@countStart");
                XmlNode renameByCountStop = renameByCountNode.SelectSingleNode("@countEnd");
                XmlNode renameByCountMath = renameByCountNode.SelectSingleNode("@math");
                XmlNode renameByCountFile = renameByCountNode.SelectSingleNode("@countFile");
                task.enableRenamingByCount = parseBooleanFromString(renameByCountEnable.Value);
                task.renameByCountPrefix = renameByCountPrefix.Value;
                task.renameByCountSuffix = renameByCountSuffix.Value;
                task.countStart = renameByCountStart.Value;
                task.countStop = renameByCountStop.Value;
                task.countMath = renameByCountMath.Value;
                task.renameByCountFile = renameByCountFile.Value;

                //task backup
                XmlNode backupNode = tasknode.SelectSingleNode("backup");
                XmlNode backupEnable = backupNode.SelectSingleNode("@enable");
                task.enableBackup = parseBooleanFromString(backupEnable.Value);
                XmlNodeList backupPathList = backupNode.SelectNodes("path");
                String[] backupPaths = new String[backupPathList.Count];
                Boolean[] backupEnables = new Boolean[backupPathList.Count];
                String[] backupModels = new string[backupPathList.Count];
                int backupIterator = 0;
                foreach (XmlNode backupPath in backupPathList) {
                    XmlNode backupPathValue = backupPath.SelectSingleNode("@value");
                    XmlNode backupPathEnable = backupPath.SelectSingleNode("@enable");
                    XmlNode backupPathBackupModeReference = backupPath.SelectSingleNode("@backupModeRef");
                    backupPaths[backupIterator] = backupPathValue.Value;
                    backupEnables[backupIterator] = parseBooleanFromString(backupPathEnable.Value);
                    backupModels[backupIterator] = backupPathBackupModeReference.Value;
                    backupIterator++;
                }
                task.backupPathValue = backupPaths;
                task.backupPathEnable = backupEnables;
                task.backupModeRef = backupModels;

                //task copy
                XmlNode copyNode = tasknode.SelectSingleNode("copylist");
                XmlNode copyEnable = copyNode.SelectSingleNode("@enable");
                task.enableCopyList = parseBooleanFromString(copyEnable.Value);
                XmlNodeList copyPathList = copyNode.SelectNodes("path");
                String[] copyPaths = new String[copyPathList.Count];
                Boolean[] copyPathsEnable = new Boolean[copyPathList.Count];
                Boolean[] copyPathsOverrides = new Boolean[copyPathList.Count];
                int copyIterator = 0;
                foreach (XmlNode copyPath in copyPathList) {
                    XmlNode copyPathValue = copyPath.SelectSingleNode("@value");
                    XmlNode copyPathEnable = copyPath.SelectSingleNode("@enable");
                    XmlNode copyPathOverride = copyPath.SelectSingleNode("@override");
                    copyPaths[copyIterator] = copyPathValue.Value;
                    copyPathsEnable[copyIterator] = parseBooleanFromString(copyPathEnable.Value);
                    copyPathsOverrides[copyIterator] = parseBooleanFromString(copyPathOverride.Value);
                    copyIterator++;
                }
                task.copyPathValue = copyPaths;
                task.copyPathEnable = copyPathsEnable;
                task.copyPathOverride = copyPathsOverrides;

                //task move
                XmlNode moveNode = tasknode.SelectSingleNode("move");
                XmlNode movePath = moveNode.SelectSingleNode("@path");
                XmlNode moveEnable = moveNode.SelectSingleNode("@enable");
                XmlNode moveOverride = moveNode.SelectSingleNode("@override");
                task.moveEnable = parseBooleanFromString(moveEnable.Value);
                task.movePath = movePath.Value;
                task.moveOverride = parseBooleanFromString(moveOverride.Value);

                //task delete
                XmlNode deleteNode = tasknode.SelectSingleNode("delete");
                XmlNode deleteEnable = deleteNode.SelectSingleNode("@enable");
                XmlNode deleteKeepFile = deleteNode.SelectSingleNode("@keepFile");
                task.enableDelete = parseBooleanFromString(deleteEnable.Value);
                task.keepFile = deleteKeepFile.Value;
                tasks[taskIterator] = task;
                taskIterator++;
            }

            XmlNodeList backupmodes = xreader.SelectNodes("config/backupModes/backupMode");
            backupController = new BackupController();
            BackupMode[] modes = new BackupMode[backupmodes.Count];
            int backupmodeIterator = 0;
            foreach (XmlNode backupmode in backupmodes) {
                BackupMode mode = new BackupMode();
                XmlNode backupModeName = backupmode.SelectSingleNode("@name");
                mode.BackupModeName = backupModeName.Value;
                XmlNodeList backupFolders = backupmode.SelectNodes("folder");
                String[] Foldernames = new string[backupFolders.Count];
                String[] FolderFileAttributes = new string[backupFolders.Count];

                for (int i = 0; i < backupFolders.Count; i++) {
                    XmlNode backupFolderName = backupFolders.Item(i).SelectSingleNode("@name");
                    XmlNode backupFolderFileAttribute = backupFolders.Item(i).SelectSingleNode("@fileAttribute");
                    Foldernames[i] = backupFolderName.Value;
                    FolderFileAttributes[i] = backupFolderFileAttribute.Value;
                }
                mode.foldernames = Foldernames;
                mode.fileAttributes = FolderFileAttributes;
                modes[backupmodeIterator] = mode;
                backupmodeIterator++;
            }
            backupController.backupModes = modes;

            XmlNodeList TimeTables = xreader.SelectNodes("config/timeTables/timeTable");
            timeTableEngine = new TimeTableEngine();
            TimeTable[] timeTables = new TimeTable[TimeTables.Count];
            for (int t = 0; t < timeTables.Length; t++) {
                TimeTable tableRun = new TimeTable();
                XmlNode tableNameRef = TimeTables.Item(t).SelectSingleNode("@name");
                tableRun.tableName = tableNameRef.Value;
                XmlNodeList yearElements = TimeTables.Item(t).SelectNodes("year");
                TimeTableYearElement[] year = new TimeTableYearElement[yearElements.Count];
                Console.WriteLine(year.Length);
                for (int y = 0; y < year.Length; y++) {
                    XmlNode yearStart = yearElements.Item(y).SelectSingleNode("@start");
                    XmlNode yearStop = yearElements.Item(y).SelectSingleNode("@end");
                    TimeTableYearElement yearRun = new TimeTableYearElement();
                    yearRun.startValue = int.Parse(yearStart.Value);
                    yearRun.stopValue = int.Parse(yearStop.Value);
                    XmlNodeList monthElements = yearElements.Item(y).SelectNodes("month");
                    TimeTableMonthElement[] month = new TimeTableMonthElement[monthElements.Count];
                    for (int m = 0; m < month.Length; m++) {
                        XmlNode monthStart = monthElements.Item(m).SelectSingleNode("@start");
                        XmlNode monthStop = monthElements.Item(m).SelectSingleNode("@end");
                        TimeTableMonthElement monthRun = new TimeTableMonthElement();
                        monthRun.startValue = int.Parse(monthStart.Value);
                        monthRun.stopValue = int.Parse(monthStop.Value);
                        XmlNodeList dayElements = monthElements.Item(m).SelectNodes("day");
                        TimeTableDayElement[] day = new TimeTableDayElement[dayElements.Count];
                        for (int d = 0; d < day.Length; d++) {
                            TimeTableDayElement dayRun = new TimeTableDayElement();
                            XmlNode dayStart = dayElements.Item(d).SelectSingleNode("@start");
                            XmlNode dayStop = dayElements.Item(d).SelectSingleNode("@end");
                            XmlNode monStart = dayElements.Item(d).SelectSingleNode("monday/@start");
                            XmlNode monStop = dayElements.Item(d).SelectSingleNode("monday/@end");
                            XmlNode tueStart = dayElements.Item(d).SelectSingleNode("tuesday/@start");
                            XmlNode tueStop = dayElements.Item(d).SelectSingleNode("tuesday/@end");
                            XmlNode wedStart = dayElements.Item(d).SelectSingleNode("wednesday/@start");
                            XmlNode wedStop = dayElements.Item(d).SelectSingleNode("wednesday/@end");
                            XmlNode thuStart = dayElements.Item(d).SelectSingleNode("thursday/@start");
                            XmlNode thuStop = dayElements.Item(d).SelectSingleNode("thursday/@end");
                            XmlNode friStart = dayElements.Item(d).SelectSingleNode("friday/@start");
                            XmlNode friStop = dayElements.Item(d).SelectSingleNode("friday/@end");
                            XmlNode satStart = dayElements.Item(d).SelectSingleNode("saturday/@start");
                            XmlNode satStop = dayElements.Item(d).SelectSingleNode("saturday/@end");
                            XmlNode sunStart = dayElements.Item(d).SelectSingleNode("sunday/@start");
                            XmlNode sunStop = dayElements.Item(d).SelectSingleNode("sunday/@end");
                            dayRun.startValue = int.Parse(dayStart.Value);
                            dayRun.stopValue = int.Parse(dayStop.Value);
                            dayRun.mondayStart = monStart.Value;
                            dayRun.mondayStop = monStop.Value;
                            dayRun.tuesdayStart = tueStart.Value;
                            dayRun.tuesdayStop = tueStop.Value;
                            dayRun.wednesdayStart = wedStart.Value;
                            dayRun.wednesdayStop = wedStop.Value;
                            dayRun.thursdayStart = thuStart.Value;
                            dayRun.thursdayStop = thuStop.Value;
                            dayRun.fridayStart = friStart.Value;
                            dayRun.fridayStop = friStop.Value;
                            day[d] = dayRun;
                        }
                        monthRun.dayElements = day;
                        month[m] = monthRun;
                    }
                    yearRun.monthElements = month;
                    year[y] = yearRun;
                }
                tableRun.yearElements = year;
                timeTables[t] = tableRun;
            }
            timeTableEngine.timeTables = timeTables;

        }
        public long parseTimeString2Mil(String timeiterationin)
        {
            String timeiteration = timeiterationin.Replace(" ", String.Empty);
            long outlong = 30000;

            if (timeiteration.Contains("sec") || timeiteration.Contains("Sec") || timeiteration.Contains("SEC") || timeiteration.Contains("S") || timeiteration.Contains("s"))
            {
                timeiteration = timeiteration.Replace("s", String.Empty);
                timeiteration = timeiteration.Replace("e", String.Empty);
                timeiteration = timeiteration.Replace("c", String.Empty);
                timeiteration = timeiteration.Replace("S", String.Empty);
                timeiteration = timeiteration.Replace("E", String.Empty);
                timeiteration = timeiteration.Replace("C", String.Empty);

                outlong = (long.Parse(timeiteration)) * 1000;

            }
            else if (timeiteration.Contains("min") || timeiteration.Contains("Min") || timeiteration.Contains("MIN") || timeiteration.Contains("M") || timeiteration.Contains("m"))
            {
                timeiteration = timeiteration.Replace("m", String.Empty);
                timeiteration = timeiteration.Replace("i", String.Empty);
                timeiteration = timeiteration.Replace("n", String.Empty);
                timeiteration = timeiteration.Replace("M", String.Empty);
                timeiteration = timeiteration.Replace("I", String.Empty);
                timeiteration = timeiteration.Replace("N", String.Empty);

                outlong = ((long.Parse(timeiteration)) * 1000) * 60;
            }
            else if (timeiteration.Contains("hour") || timeiteration.Contains("Hour") || timeiteration.Contains("HOUR") || timeiteration.Contains("H") || timeiteration.Contains("h"))
            {
                timeiteration = timeiteration.Replace("h", String.Empty);
                timeiteration = timeiteration.Replace("o", String.Empty);
                timeiteration = timeiteration.Replace("u", String.Empty);
                timeiteration = timeiteration.Replace("r", String.Empty);
                timeiteration = timeiteration.Replace("H", String.Empty);
                timeiteration = timeiteration.Replace("O", String.Empty);
                timeiteration = timeiteration.Replace("U", String.Empty);
                timeiteration = timeiteration.Replace("R", String.Empty);

                outlong = (((long.Parse(timeiteration)) * 1000) * 60) * 60;
            }
            else {

            }

            return outlong;
        }

        public Boolean parseBooleanFromString(String input) {

            Boolean outBool = false;

            if (input.Contains("true") || input.Contains("TRUE") || input.Contains("1") || input.Contains("True"))
            {
                outBool = true;
            }
            else {
                outBool = false;
            }

            return outBool;
        }
    }

    class Task {
        //task
        public String taskid { get; set; }
        public Boolean taskEnable { get; set; }
        //timing
        public long milsecIteration { get; set; }
        public String timeTableRef { get; set; }
        //input
        public String inputPath { get; set; }
        public String inputMask { get; set; }
        public int inputDepth { get; set; }
        public Boolean recusive { get; set; }
        //temp
        public Boolean enableTemporary { get; set; }
        public String tempPath { get; set; }
        public String tempMask { get; set; }
        //renameByDate
        public Boolean enableRenameByDate { get; set; }
        public String renameByDatePrefix { get; set; }
        public String renameByDateNameCondition { get; set; }
        public String renameByDateSuffix { get; set; }
        //renameByCount
        public Boolean enableRenamingByCount { get; set; }
        public String renameByCountPrefix { get; set; }
        public String renameByCountFile { get; set; }
        public String renameByCountSuffix { get; set; }
        public String countStart { get; set; }
        public String countStop { get; set; }
        public String countMath { get; set; }
        //backup
        public Boolean enableBackup { get; set; }
        public String[] backupPathValue { get; set; }
        public Boolean[] backupPathEnable { get; set; }
        public String[] backupModeRef { get; set; }
        //copy
        public Boolean enableCopyList { get; set; }
        public String[] copyPathValue { get; set; }
        public Boolean[] copyPathEnable { get; set; }
        public Boolean[] copyPathOverride { get; set; }
        //move
        public String movePath { get; set; }
        public Boolean moveEnable { get; set; }
        public Boolean moveOverride { get; set; }
        //delete
        public Boolean enableDelete { get; set; }
        public String keepFile { get; set; }

        public void fullaction() {
            Program pro = new Program();
            BackupController bc = Program.cif.backupController;

            if (taskEnable)
            {
                if (enableTemporary)
                {
                    String outputExtension = getOutputExtension(inputPath, inputMask);
                    while (!String.IsNullOrEmpty(outputExtension))
                    {
                        outputExtension = getOutputExtension(inputPath, inputMask);

                        if (String.IsNullOrEmpty(outputExtension))
                        {
                            return;
                        }
                        String TempFile = temporaryFileMovement(inputPath, inputMask, tempMask, tempPath);
                        if (enableRenameByDate)
                        {
                            TempFile = renameByDate(TempFile);
                            outputExtension = Path.GetExtension(TempFile);
                        }
                        else if (enableRenamingByCount)
                        {
                            TempFile = renameByCount(TempFile);
                            outputExtension = Path.GetExtension(TempFile);
                        }
                        else { }
                        if (enableBackup) {
                            for (int bci = 0; bci < backupModeRef.Length; bci++) {
                                if (backupPathEnable[bci])
                                {
                                    String buildBackup = backupPathValue[bci] + bc.getFolderBackupPath(TempFile, backupModeRef[bci]) + "\\" + Path.GetFileName(TempFile);
                                    Directory.CreateDirectory(Path.GetDirectoryName(buildBackup));
                                    File.Copy(TempFile, buildBackup, true);
                                }
                                    }
                        }

                        if (enableCopyList)
                        {
                            copyOnList(TempFile, outputExtension);
                        }
                        if (moveEnable)
                        {
                            Movefile(TempFile, outputExtension);
                        }
                        deleteFile(TempFile);
                    }
                }
                else {
                    String outputExt = getOutputExtension(inputPath, inputMask);
                    while (!String.IsNullOrEmpty(outputExt))
                    {
                        outputExt = getOutputExtension(inputPath, inputMask);
                        if (String.IsNullOrEmpty(outputExt)) {
                            return;
                        }
                        String workFile = simpleFileMovement(inputPath, inputMask);
                        if (enableRenameByDate)
                        {
                            workFile = renameByDate(workFile);
                            outputExt = Path.GetExtension(workFile);
                        }
                        else if (enableRenamingByCount)
                        {
                            workFile = renameByCount(workFile);
                            outputExt = Path.GetExtension(workFile);
                        }
                        else { }
                        if (enableBackup)
                        {
                            for (int bci = 0; bci < backupModeRef.Length; bci++)
                            {
                                if (backupPathEnable[bci])
                                {
                                    String buildBackup = backupPathValue[bci] + bc.getFolderBackupPath(workFile, backupModeRef[bci]) + "\\" + Path.GetFileName(workFile);
                                    Directory.CreateDirectory(Path.GetDirectoryName(buildBackup));
                                    File.Copy(workFile, buildBackup, true);
                                }
                            }
                        }
                        if (enableCopyList)
                        {
                            copyOnList(workFile, outputExt);
                        }
                        if (moveEnable)
                        {
                            Movefile(workFile, outputExt);
                        }
                        deleteFile(workFile);
                    }
                }


            }
            else {

                Console.WriteLine("task not enabled: " + taskid);
            }


        }


        public String temporaryFileMovement(String Source, String wildcard, String tempmask, String Tempfolder) {
            //getFile
            String filestr = getSimpleFileList(Source, wildcard)[0];
            Console.WriteLine("got File: " + filestr);
            //generate new filename
            //splitfilenam by . get front and 
            String filename = Path.GetFileNameWithoutExtension(filestr);
            String tempFileSourceName = inputPath + "\\" + filename + tempMask;
            String tempFileTempDestName = tempPath + "\\" + filename + tempMask;
            File.Move(filestr, tempFileSourceName);
            File.Copy(tempFileSourceName, tempFileTempDestName, true);
            //checkout to Tempfolder
            //return Filename in tempfolder
            return tempFileTempDestName;
        }
        public String simpleFileMovement(String Source, String wildcard) {
            //getFile
            String filestr = getSimpleFileList(Source, wildcard)[0];
            Console.WriteLine("got File: " + filestr);
            //generate new filename
            return filestr;
        }

        public String renameByDate(string source) {
            String sourcePath = Path.GetDirectoryName(source);
            String DateName = DateTime.Now.ToString(renameByDateNameCondition);
            String newFileName = sourcePath + "\\" + renameByDatePrefix + DateName + renameByDateSuffix;
            File.Move(source, newFileName);

            return newFileName;
        }

        public String renameByCount(string source) {
            String sourcePath = Path.GetDirectoryName(source);
            String oldCount = "";
            String newCount = "";
            if (!File.Exists(renameByCountFile))
            {
                newCount = countStart;
            }
            else
            {
                using (StreamReader sr = File.OpenText(renameByCountFile))
                {
                    oldCount = "";
                    if ((oldCount = sr.ReadLine()) != null)
                    {
                        if (int.Parse(oldCount) > int.Parse(countStop))
                        {
                            newCount = countStart;
                        }
                        Console.WriteLine("Read Count: " + oldCount);
                    }
                    else
                    {
                        newCount = countStart;
                    }
                }
            }
            if (String.IsNullOrEmpty(newCount))
            {
                int oldCountInt = int.Parse(oldCount);
                string mathCondition = countMath.Replace("$", oldCountInt.ToString());
                DataTable dt = new DataTable();
                newCount = dt.Compute(mathCondition, "").ToString();
                Console.WriteLine("new Count is: " + newCount);
            
        }
        using (StreamWriter swt = new StreamWriter(renameByCountFile,false, Encoding.UTF8))
                {
                    swt.Write(newCount);

                }
            String outputFileName = sourcePath + "\\" + renameByCountPrefix + newCount + renameByCountSuffix;
            File.Move(source, outputFileName);


            return outputFileName;
        }

        public void copyOnList(string source, string outputExtension) {
            if (!Directory.Exists(Path.GetDirectoryName(source)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(source));
            }
            for (int i = 0; i < copyPathValue.Length; i++) {
                if (copyPathEnable[i]) {
                    if (!Directory.Exists(copyPathValue[i]))
                    {
                        Directory.CreateDirectory(copyPathValue[i]);
                    }
                    if (enableTemporary)
                    {
                        if (source.Contains(tempMask))
                        {
                            String simpleFilename = Path.GetFileNameWithoutExtension(source);
                            String newFilename = copyPathValue[i] +"\\" + simpleFilename + tempMask;
                            File.Copy(source, newFilename,copyPathOverride[i]);
                            String FinalFile = copyPathValue[i] + "\\" + simpleFilename + outputExtension;
                            File.Move(newFilename, FinalFile);
                        }
                        else {
                            String getnewfileExtension = Path.GetExtension(source);
                            String simpleFilename = Path.GetFileNameWithoutExtension(source);
                            String newFilename = copyPathValue[i] + "\\" + simpleFilename + tempMask;
                            File.Copy(source, newFilename, copyPathOverride[i]);
                            String FinalFile = copyPathValue[i] + "\\" + simpleFilename + getnewfileExtension;
                            File.Move(newFilename, FinalFile);
                        }
                    }
                    String getnewfileExtensionnoTemp = Path.GetExtension(source);
                    String simpleFilenameNoTemp = Path.GetFileNameWithoutExtension(source);
                    String newFilenameNoTemp = copyPathValue[i] + "\\" + simpleFilenameNoTemp +getnewfileExtensionnoTemp ;
                    File.Copy(source, newFilenameNoTemp, copyPathOverride[i]);
                }
            }
        }

        public void Movefile(string source, string outputExtension) {
            if (!Directory.Exists(movePath))
            {
                Directory.CreateDirectory(movePath);
            }
            if (enableTemporary)
            {
                if (source.Contains(tempMask))
                {
                    String simpleFilename = Path.GetFileNameWithoutExtension(source);
                    String newFilename = movePath + "\\" + simpleFilename + tempMask;
                    File.Copy(source, newFilename,moveOverride);
                    String FinalFile = movePath + "\\" + simpleFilename + outputExtension;
                    File.Move(newFilename, FinalFile);
                }
                else
                {
                    String getnewfileExtension = Path.GetExtension(source);
                    String simpleFilename = Path.GetFileNameWithoutExtension(source);
                    String newFilename = movePath + "\\" + simpleFilename + tempMask;
                    File.Copy(source, newFilename, moveOverride);
                    String FinalFile = movePath + "\\" + simpleFilename + getnewfileExtension;
                    File.Move(newFilename, FinalFile);
                }
            }
            String getnewfileExtensionnoTemp = Path.GetExtension(source);
            String simpleFilenameNoTemp = Path.GetFileNameWithoutExtension(source);
            String newFilenameNoTemp = movePath + "\\" + simpleFilenameNoTemp + getnewfileExtensionnoTemp;
            File.Copy(source, newFilenameNoTemp, moveOverride);
        }

        public void deleteFile(string source) {
            if (enableDelete)
            {
                //find file in input 
                String[] inputFilesStandard = Directory.GetFiles(inputPath, inputMask);
                String[] inputFilesTemp = Directory.GetFiles(inputPath, inputMask);
                String[] tempFiles = Directory.GetFiles(inputPath);
                String naturalSource = Path.GetFileNameWithoutExtension(source);
                for (int i = 0; i < inputFilesStandard.Length; i++) {
                    if (inputFilesStandard[i].Contains(naturalSource))
                    {
                        File.Delete(inputFilesStandard[i]);
                    }
                }
                for (int i = 0; i < inputFilesTemp.Length; i++) {
                    if (inputFilesTemp[i].Contains(naturalSource))
                    {
                        File.Delete(inputFilesTemp[i]);
                    }
                }
                for (int i = 0; i < tempFiles.Length; i++) {
                    if (tempFiles[i].Contains(naturalSource))
                    {
                        File.Delete(tempFiles[i]);
                    }
                }
                //Find file in temp
            }
            else {
                //make list if not existent
                using (StreamWriter swt = new StreamWriter(keepFile, true, Encoding.UTF8))
                {
                    swt.WriteLine(source);

                }
                //read every time for update
                //delete unused exceptions

            }

        }

        public Boolean checkforActionKeep(string source) {

            //iflist is existent
            //search for every item if already moved or copied
            //make list if not existent
            return true;
        }

        public void CopyFiles(string sourceDir,String wildcard, string targetDir, Boolean recursive, int depth)
        {

            string[] files = Directory.GetFiles(sourceDir,wildcard);
            foreach (var fileName in files)
            {
                string targetFile = Path.Combine(targetDir, (new FileInfo(fileName)).Name);
                if (File.Exists(targetFile) == false)
                    File.Copy(fileName, targetFile);
            }
        }

        public List<String> getSimpleFileList(String sourcedir, String wildcard) {
            var files = new List<string>();
            files.AddRange(Directory.EnumerateFiles(sourcedir, wildcard));
            return files;

        }

        public Tuple<List<string>, List<string>> getFilesAndFoldersInDepth(string sourceDir, String wildcard, int depth) {

            var folders = new List<string>();
            var files = new List<string>();
            foreach (var directory in Directory.EnumerateDirectories(sourceDir))
            {
                folders.Add(directory);
                if (depth > 0)
                {
                    var result = getFilesAndFoldersInDepth(directory,wildcard, depth - 1);
                    folders.AddRange(result.Item1);
                    files.AddRange(result.Item2);
                }
            }

            files.AddRange(Directory.EnumerateFiles(sourceDir,wildcard));

            return new Tuple<List<String>, List<String>>(folders, files);

        }
        public String getOutputExtension(string source, string wildcard) {
          
          
                Directory.CreateDirectory(Path.GetFullPath(source));
           
            String[] files = Directory.GetFiles(source, wildcard);
            if (files.Length < 1)return null;
           
            
            
                string output = Path.GetExtension(files[0]);
                return output;
            
        }
    }

    class TimeTableEngine {

        public TimeTable[] timeTables{ get; set; }


        public Boolean inTableRange(String TableReference) {

            return true;
        }

    }

    class TimeTable {

        public String tableName{ get; set; }
        public TimeTableYearElement[] yearElements{ get; set; }


    }

    class TimeTableYearElement {
        public TimeTableMonthElement[] monthElements{ get; set; }
        public int startValue { get; set; }
        public int stopValue { get; set; }
        public int runningValue { get; set; }
        }

    class TimeTableMonthElement {
        public TimeTableDayElement[] dayElements{ get; set; }
        public int startValue { get; set; }
        public int stopValue { get; set; }
        public int runningValue { get; set; }
    }

    class TimeTableDayElement {
        public int startValue{ get; set; }
        public int stopValue{ get; set; }
        public int runningValue{ get; set; }

        //monday
        public String mondayStart{ get; set; }
        public String mondayStop{ get; set; }
        //tuesday
        public String tuesdayStart{ get; set; }
        public String tuesdayStop{ get; set; }
        //wednesday
        public String wednesdayStart{ get; set; }
        public String wednesdayStop{ get; set; }
        //thursday
        public String thursdayStart{ get; set; }
        public String thursdayStop{ get; set; }
        //friday
        public String fridayStart{ get; set; }
        public String fridayStop{ get; set; }
        //Saturday
        public String saturdayStart{ get; set; }
        public String saturdayStop{ get; set; }
        //Sunday
        public String sundayStart{ get; set; }
        public String sundayStop{ get; set; }

    }

    class BackupController {

        public BackupMode[] backupModes;



        public String getFolderBackupPath(String Filepath, String backupModeRef) {
            for (int m =0; m < backupModes.Length; m++)
            {
                if (backupModeRef.Equals(backupModes[m].BackupModeName)) {
                    return backupModes[m].generateBackupPath(Filepath);
                }
            }
            return "";
        }
    }

    class BackupMode {
        public String[] foldernames { get; set; }
        public String[] fileAttributes { get; set; }
        public String BackupModeName { get; set; }
        
        public String generateBackupPath( String file) {
            String backupPathSegment = "";
            
            for (int f = 0; f < foldernames.Length; f++) {
                //do SpareFolder String for Path Segment
                String SpareFolder = "";
                //get Attributes from FoldernameString
                String[] attributes = getAttributeArrayFromString(foldernames[f]);
                Console.WriteLine("Found Length of Attributes: " + attributes.Length);
                //get Attributes by Split by +
                String[] attributeSettings = fileAttributes[f].Split('+');
                Console.WriteLine("Found Length of AttributesSettings: " + attributeSettings.Length);
                //get conjunctions by Foldername
                String[] conjunction = getConjunctionArrayFromString(foldernames[f]);
                Console.WriteLine("Found Length of conjunctions: " + conjunction.Length);
                //get SpareFoldername for navigation either attribute or Conjunction will Start Foldername
                //by removing the [] there will be only plain Text for navigation
                String spareFolderName = foldernames[f].Replace("[", string.Empty);
                spareFolderName = spareFolderName.Replace("]", string.Empty);
                Console.WriteLine("sparefoldername contains: " + spareFolderName);
                Console.WriteLine("sparefoldername has length: " + spareFolderName.Length);
                //the Length of every Array will be simulated by following objects
                //the functioning is to ask the difference between those simulated length which will be afterwards dynamically changed by use and the real Array Length
                int a = attributes.Length-1;
                int arun = 0;
                int c = conjunction.Length-1;
                int crun=0;
                int s = attributeSettings.Length - 1;
                int srun = 0;
                //the Loop has go through both arrays the conjunction and the attributes array
                for (int n = 0; n < (attributes.Length + conjunction.Length); n++) {
                    if (String.IsNullOrEmpty(spareFolderName)) {
                        continue;
                    }
                    if (spareFolderName.StartsWith(attributes[arun]) && attributes.Length>0) {
                        if (attributeSettings[srun].Contains("modification"))
                        {
                            SpareFolder = SpareFolder +"\\"+ getModificationTime(attributes[arun], file);
                            Console.WriteLine(SpareFolder);
                        }
                        else if (attributeSettings[srun].Contains("creation"))
                        {
                            SpareFolder = SpareFolder + "\\" + getCreationTime(attributes[arun], file);
                            Console.WriteLine(SpareFolder);
                        }
                        else if (attributeSettings[srun].Contains("substring"))
                        {
                            SpareFolder = SpareFolder + "\\" + getSubstring(attributes[arun], file);
                            Console.WriteLine(SpareFolder);
                        }
                        else if (attributeSettings[srun].Contains("split"))
                        {
                            SpareFolder = SpareFolder + "\\" + getSplit(attributes[arun], file);
                            Console.WriteLine(SpareFolder);
                        }
                        spareFolderName = spareFolderName.Remove(0, attributes[arun].Length);
                        srun++;
                        arun++;
                    } else if (spareFolderName.StartsWith(conjunction[crun]) && conjunction.Length > 0) {
                        SpareFolder = SpareFolder + "\\" + conjunction[crun];
                        Console.WriteLine(SpareFolder);
                        spareFolderName = spareFolderName.Remove(0, conjunction[crun].Length);
                        crun++;
                    }
                    
                }
                backupPathSegment = backupPathSegment + SpareFolder;
                Console.WriteLine(backupPathSegment);
            }

            return backupPathSegment;
        }

        public String[] getAttributeArrayFromString( String input) {
            String[] attributes;
            List<String> spareAttributeList = new List<string>();
            String spareAttribute = "";
            Char[] inputCharArray = input.ToCharArray();
            Console.WriteLine("lenght of Char array: "+inputCharArray.Length);
            Boolean WritingAttribute = false;
            for (int i = 0; i < inputCharArray.Length; i++) {
                if (inputCharArray[i].Equals('['))
                {
                    Console.WriteLine("telemetry found [ in expression will start capturing");
                    WritingAttribute = true;
                    continue;
                }
                else if (inputCharArray[i].Equals(']'))
                {
                    Console.WriteLine("telemetry found ] in expression will stop capturing");
                    spareAttributeList.Add(spareAttribute);
                    Console.WriteLine(spareAttribute);
                    spareAttribute = "";
                    WritingAttribute = false;
                    continue;
                }
                if (WritingAttribute) {
                    spareAttribute = spareAttribute + inputCharArray[i];
                }
            }
            attributes =  spareAttributeList.ToArray();
            Console.WriteLine("attributes length: " + attributes.Length);
            return attributes;
        }
        public String[] getConjunctionArrayFromString(String input) {

            String[] conjunction;
            List<String> spareConjunctionList = new List<string>();
            String spareConjunction = "";
            Char[] inputCharArray = input.ToCharArray();
            Boolean WritingConjunction = false;
            for (int i = 0; i < inputCharArray.Length; i++)
            {
                if (inputCharArray[i].Equals(']'))
                {
                    Console.WriteLine("telemetry found ] in expression will start capturing conjunction");
                    WritingConjunction = true;
                    continue;
                }
                else if (inputCharArray[i].Equals('['))
                {
                    Console.WriteLine("telemetry found [ in expression will stop capturing conjunction");
                    spareConjunctionList.Add(spareConjunction);
                    spareConjunction = "";
                    WritingConjunction = false;
                    continue;
                }
                else { }
                if (WritingConjunction)
                {
                    spareConjunction = spareConjunction + inputCharArray[i];
                }
            }
            conjunction = spareConjunctionList.ToArray();

            return conjunction;


        }

        public String getModificationTime(String complex, String file) {
           String output= File.GetLastWriteTime(file).ToString(complex);
            Console.WriteLine(output);
            return output;
        }

        public String getCreationTime(String complex, String file) {
            string output = File.GetCreationTime(file).ToString(complex);
            Console.WriteLine(output);
            return output;
        }

        public String getSubstring(String complex, String file) {
            String Filename = Path.GetFileNameWithoutExtension(file);
            String[] complexarray = complex.Split('-');
            int length = int.Parse(complexarray[1]) - int.Parse(complexarray[0]);
            String output = Filename.Substring(int.Parse(complexarray[0]), length);
            return output;
        }
        public String getSplit(String complex, String file) {
            String Filename = Path.GetFileNameWithoutExtension(file);
            String[] complexArray = complex.Split(':');
            String[] fileNameArray = Filename.Split(Char.Parse(complexArray[1]));
            String output = fileNameArray[int.Parse(complexArray[0])];
            return output;
        }
    }
}
