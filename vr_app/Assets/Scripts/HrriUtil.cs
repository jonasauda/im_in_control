using System;
using System.Text.RegularExpressions;
using static Config;

public class HrriUtil
{
    public static string HrriLocationToString(HrriLocation hrriLocation) {
        return Enum.GetName(typeof(HrriLocation), hrriLocation);
    }

    public static string HrriGroupToString(HrriGroup hrriGroup) {
        return Enum.GetName(typeof(HrriGroup), hrriGroup);
    }

    public static string HrriObjectToString(HrriObject hrriObject) {
        return Enum.GetName(typeof(HrriObject), hrriObject);

    }

    public static string CreateHrri(HrriLocation hrriLocation, HrriGroup hrriGroup, HrriObject hrriObject, string hrriAttribute) {
        string hrriLocationString = HrriLocationToString(hrriLocation);
        string hrriGroupString = HrriGroupToString(hrriGroup);
        string hrriObjectString = HrriObjectToString(hrriObject);
        if (hrriAttribute != null && hrriAttribute != "") {
            return String.Format("{0}-{1}-{2}-{3}", hrriLocationString, hrriGroupString, hrriObjectString, hrriAttribute);
        } else {
            return String.Format("{0}-{1}-{2}", hrriLocationString, hrriGroupString, hrriObjectString);
        }
    }

    public static string CreateHrri(string hrriLocation, string hrriGroup, string hrriObject, string hrriAttribute) {
        string hrri = "";
        if (hrriLocation == null) {
            return hrri;
        }
        hrri += hrriLocation.ToUpper();
        if (hrriGroup == null || hrriGroup.Equals("")) {
            return hrri;
        }
        hrri += "-" + hrriGroup.ToUpper();
        if (hrriObject == null || hrriObject.Equals("")) {
            return hrri;
        }
        hrri += "-" + hrriObject.ToUpper();
        if (hrriAttribute == null || hrriAttribute.Equals("")) {
            return hrri;
        }
        hrri += "-" + hrriAttribute.ToUpper();
        return hrri;
    }

    public static bool IsWellFormedHrri(String hrri) {
        return Regex.IsMatch(hrri, "[A-Z]+-[A-Z]+-[A-Z]+(-[A-Z]+)?");
    }

    public static (HrriLocation, HrriGroup, HrriObject, string) ParseHrri(string hrri) {
        HrriLocation hrriLocation = HrriLocation.NONE;
        HrriGroup hrriGroup = HrriGroup.NONE;
        HrriObject hrriObject = HrriObject.NONE;
        string hrriAttribute = "";

        string[] hrriParts = hrri.Split('-');
        Enum.TryParse(hrriParts[0], out hrriLocation);
        if (hrriParts.Length > 1) {
            Enum.TryParse(hrriParts[1], out hrriGroup);
        }
        if (hrriParts.Length > 2) {
            Enum.TryParse(hrriParts[2], out hrriObject);
        }
        if (hrriParts.Length > 3) {
            hrriAttribute = hrriParts[3];
        }

        return (hrriLocation, hrriGroup, hrriObject, hrriAttribute);
    }

    public static bool MaskFitsHrri(HrriMask hrriMask, string hrri) {
        return IsWellFormedHrri(hrri) && hrri.StartsWith(hrriMask.GetHrriMaskPrefix());
    }

    public static bool HrriObjectEquals(string hrri, string objectName) {
        string[] hrriParts = hrri.Split('-');
        if (hrriParts.Length < 3) {
            return false;
        }
        return hrriParts[2].Equals(objectName);
    }

    public class HrriMask {
        private HrriLocation hrriLocation;
        private HrriGroup hrriGroup;

        private string hrriLocationString;
        private string hrriGroupString;
        private string hrriMaskPrefix;

        public HrriMask(HrriLocation hrriLocation, HrriGroup hrriGroup) {
            this.hrriLocation = hrriLocation;
            this.hrriGroup = hrriGroup;
            hrriLocationString = Enum.GetName(typeof(HrriLocation), hrriLocation);
            hrriGroupString = Enum.GetName(typeof(HrriGroup), hrriGroup);
            hrriMaskPrefix = hrriLocationString + "-" + hrriGroupString;
        }

        public string GetHrriMaskPrefix() {
            return hrriMaskPrefix;
        }
    }

}
