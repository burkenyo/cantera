// This file is part of Cantera. See License.txt in the top-level directory or
// at https://cantera.org/license.txt for license and copyright information.

[assembly: AssemblyFixture(typeof(Cantera.Tests.ApplicationFixture))]

namespace Cantera.Tests;

public class ApplicationFixture
{
    public ApplicationFixture()
    {
        Application.DataDirectories.AddAssemblyDirectory();
    }
}
