using UnityEditor;

[InitializeOnLoad]
public class XRPackageChecker : PackageChecker
{
    static XRPackageChecker()
    {
        new XRPackageChecker();
    }

    private XRPackageChecker() : base("xr", "XR_PACKAGE_INSTALLED")
    {
    }
}