using Microsoft.Win32;
using OCSS.Util.CmdLine;
using System;
using System.Linq;
using System.Security;

namespace FixTrayIcons {

   internal class Program {
      #region Command line flags
      // required flags
      public static readonly string CmdLineParm_Action = "a";
      // optional flags
      public static readonly string CmdLineParm_NameFilter = "n";
      public static readonly string CmdLineParm_Pause = "p";
      #endregion

      public static readonly string RegKeyIsPromoted = "IsPromoted";
      public static readonly string RegKeyExePath = "ExecutablePath";

      public static readonly string RegMainKeyPath = "Control Panel\\NotifyIconSettings";

      public static readonly string RegistrySecurityExceptMsg = "Cannot open registry key where Tray Icons settings are stored. Are you running as Admin?";

      private static CmdLine cmdLineParser;
      private static RunParms runParms;

      static void Main(string[] args) {
         CmdFlag[] flags = {
               // required parms
               new CmdFlag(CmdLineParm_Action, true),
               // optional parms
               new CmdFlag(CmdLineParm_NameFilter, false),
               new CmdFlag(CmdLineParm_Pause, false),
         };
         cmdLineParser = new CmdLine(flags);
         cmdLineParser.ProcessCmdLine(args);
         runParms = new RunParms() {
            UserAction = GetParmOrDefault(CmdLineParm_Action, string.Empty),
            NameFilter = GetParmOrDefault(CmdLineParm_NameFilter, string.Empty),
            PauseWhenDone = cmdLineParser.ParmExists(CmdLineParm_Pause)

         };
         var errors = runParms.GetValidationErrors().ToList();
         if (errors.Count == 0) {
            if (IsWin11OrGreater()) {
               // run update
               // Console.WriteLine($"Run ready - Action: {runParms.UserAction}, Name: {runParms.NameFilter}, Pause: {runParms.PauseWhenDone}");
               UpdateTraySettings();
               Environment.ExitCode = 0;
            }
            else {
               Console.WriteLine(RunErrors.GetErrorMessageEN(RunErrors.Err_InvalidOS));
               Environment.ExitCode = RunErrors.Err_InvalidOS;
            }
         }
         else {
            string errMsgs = string.Join("\r\n", RunErrors.GetErrorMessagesEN(errors));
            Console.WriteLine(errMsgs);
            // return the first error to calling program/cmd
            Environment.ExitCode = errors[0];
         }
         if (runParms.PauseWhenDone) {
            Console.WriteLine("Press <Enter> to close");
            Console.ReadLine();
         }
      }

      private static string GetParmOrDefault(string flag, string defaultVal) {
         return cmdLineParser.ParmExists(flag) ? cmdLineParser.GetParm(flag) : defaultVal;
      }

      private static bool IsWin11OrGreater() {
         return true;
         // Note: this doesn't work reliably as MS has deprecated some calls and changed the results of others.
         // Todo: update if MS ever gets their $hit together.
         // Console.WriteLine($"Build: {Environment.OSVersion.Version.Build}");
         // return Environment.OSVersion.Version.Build >= 22000;
      }

      /// <summary>Update Tray Settings</summary>
      /// <remarks>If this gets too large, it should be moved to its own class</remarks>
      private static void UpdateTraySettings() {
         try {
            using (RegistryKey mainTrayKey = Registry.CurrentUser.OpenSubKey(RegMainKeyPath, true)) {
               if (mainTrayKey == null) {
                  Console.WriteLine("Registry path where Tray Icons settings are stored is not found.");
               }
               else {
                  int updatedCount = 0;
                  var subKeyNames = mainTrayKey.GetSubKeyNames();
                  foreach (var subKeyName in subKeyNames) {
                     //Console.WriteLine($"{subKey}");
                     RegistryKey subkey = mainTrayKey.OpenSubKey(subKeyName, true);
                     if (subkey != null) {
                        var exePath = (string) subkey.GetValue(RegKeyExePath, string.Empty);
                        int firstSlash = exePath.IndexOf(@"\");
                        string shortenedPath = firstSlash > 0 ? exePath.Substring(firstSlash) : exePath;
                        int isShownVal = (int) subkey.GetValue(RegKeyIsPromoted, 0);
                        string isShownStr = isShownVal == 1 ? "Shown" : "Hidden";
                        if (runParms.UserAction == RunParms.ActionOpt_Display) {
                           Console.WriteLine($"{shortenedPath} = {isShownStr}");
                        }
                        else {
                           if (runParms.UserAction == RunParms.ActionOpt_Update && isShownVal != 1) {
                              // if all names are requested or name matches
                              if (runParms.NameFilter == string.Empty || shortenedPath.Contains(runParms.NameFilter)) {
                                 subkey.SetValue(RegKeyIsPromoted, 1, RegistryValueKind.DWord);
                                 updatedCount++;
                                 Console.WriteLine($"{shortenedPath} updated to show icon");
                              }
                           }
                        }
                     }
                  }
                  if (runParms.UserAction == RunParms.ActionOpt_Update) {
                     Console.WriteLine($"Updated Tray Icon settings made: {updatedCount}");
                  }
               }
            }
         }
         catch (SecurityException) {
            Console.WriteLine(RegistrySecurityExceptMsg);
         }
      }

   }

}