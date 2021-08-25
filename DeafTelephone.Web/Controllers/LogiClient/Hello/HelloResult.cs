namespace DeafTelephone.Web.Controllers.LogiClient.Hello
{
    using System.Collections.Generic;

    public class HelloResult
    {
        public readonly Dictionary<string, List<string>> EnvsToProjectsMap;

        public HelloResult()
        {
            EnvsToProjectsMap = new();
        }

        public HelloResult(Dictionary<string, List<string>> map)
        {
            EnvsToProjectsMap = map;
        }
    }
}
