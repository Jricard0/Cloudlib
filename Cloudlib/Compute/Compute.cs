using System;
namespace Cloudlib.Compute
{
    public class Compute
    {
        public ICompute client;

        public Compute(string computeProvider, string project)
        {
            switch (computeProvider)
            {
                case "gcloud":
                    client = new GoogleCompute(project);
                    break;
                default:
                    break;
            }
        }
    }
}
