using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

#region Unity Package

[assembly: AssemblyMetadata("Unity.Package.Name", "unity.serialization")]

#endregion


[assembly: InternalsVisibleTo("Unity.Serialization.Editor")]
[assembly: InternalsVisibleTo("Unity.Serialization.Tests")]
[assembly: InternalsVisibleTo("Unity.Serialization.EditorTests")]