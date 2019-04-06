using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace SDPlatform
{
    public class SDPlatformInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "SDPlatform";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Properties.Resources.SD;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("254fe933-0756-46f5-b45c-e3d305354f82");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
