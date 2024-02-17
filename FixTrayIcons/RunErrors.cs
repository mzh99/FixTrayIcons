using System.Collections.Generic;

namespace FixTrayIcons {

   public static class RunErrors {

      public static readonly int Err_NoAction = 1;
      public static readonly int Err_InvalidAction = 2;
      public static readonly int Err_InvalidOS = 99;

      public static readonly Dictionary<int, string> ErrorLookupEN = new Dictionary<int, string>() {
         { Err_NoAction, "No action specified." },
         { Err_InvalidAction, "Invalid action specified; action must be d or s." },
         { Err_InvalidOS, "Invalid OS; this runs only for Windows 11." }
   };

      public static string GetErrorMessageEN(int errCode) {
         return ErrorLookupEN.TryGetValue(errCode, out string errMsg) ? $"(Error: {errCode}) {errMsg}" : $"Unclassified error code: {errCode}";
      }

      public static IEnumerable<string> GetErrorMessagesEN(IEnumerable<int> errCodes) {
         foreach (var errCode in errCodes) {
            yield return GetErrorMessageEN(errCode);
         }

      }

   }
}
