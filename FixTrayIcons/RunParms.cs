using System.Collections.Generic;

namespace FixTrayIcons {

   public class RunParms {

      public static readonly string ActionOpt_Display = "D";
      public static readonly string ActionOpt_Update = "U";
      public static readonly List<string> AllowedActionCodes = new List<string>() { ActionOpt_Display, ActionOpt_Update };

      private string userAction;

      public string UserAction {
         get { return userAction; }
         set {
            if (string.IsNullOrEmpty(value)) {
               userAction = string.Empty;
            }
            else {
               userAction = value[0].ToString().ToUpper();
            }
         }
      }

      public string NameFilter { get; set; }
      public bool PauseWhenDone { get; set; }

      public RunParms() {
         UserAction = string.Empty;
         NameFilter = string.Empty;
         PauseWhenDone = false;
      }

      public bool IsValidActionCode { get { return userAction == string.Empty ? false : AllowedActionCodes.Contains(userAction); } }

      public IEnumerable<int> GetValidationErrors() {
         // check start folder
         if (string.IsNullOrEmpty(UserAction)) {
            yield return RunErrors.Err_NoAction;
            // return immediately for this error
            yield break;
         }
         if (IsValidActionCode == false)
            yield return RunErrors.Err_InvalidAction;
      }

   }

}
