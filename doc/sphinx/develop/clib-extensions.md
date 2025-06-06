# Generated CLib API

```{caution}
The generated CLib API is an experimental part of Cantera and may be changed
without notice.
```

In CLib, Cantera objects are stored and referenced by integers - no pointers are passed
to or from the calling application. Further, the contents of arrays and variables are
copied, which separates internal C++ memory management from the application using CLib.

**Example:** Cantera implements a method to retrieve molecular weights in C++ as the
{ct}`Phase::getMolecularWeights` class method, which is accessible from the derived C++
{ct}`ThermoPhase` class. The CLib source generator expands a corresponding
`getMolecularWeights` entry:

```yaml
- name: getMolecularWeights
  uses: nSpecies
```

in the [Header Specification File](sec-sourcegen-specifications) `ctthermo_auto.yaml`
into a CLib header within a generated `ctthermo.h` file:

```c
/**
 *  Copy the vector of molecular weights into array weights.
 *
 *  Wraps C++ getter:
 *  - `void Phase::getMolecularWeights(double*)`
 *
 *  Uses:
 *  - `size_t Phase::nSpecies()`
 *
 *  @param handle       Handle to queried Phase object.
 *  @param[in] weightsLen Length of array reserved for weights.
 *  @param weights      Output array of molecular weights (kg/kmol)
 */
int32_t thermo_getMolecularWeights(int32_t handle, int32_t weightsLen, double* weights);
```

and an associated CLib implementation within a generated `ctthermo.cpp` file:

```c
int32_t thermo_getMolecularWeights(int32_t handle, int32_t weightsLen, double* weights)
{
    // getter: void Phase::getMolecularWeights(double*)
    try {
        auto obj = ThermoPhaseCabinet::as<Phase>(handle);
        if (weightsLen < obj->nSpecies()) {
            throw ArraySizeError("thermo_getMolecularWeights", weightsLen, obj->nSpecies());
        }
        obj->getMolecularWeights(weights);
        return 0;
    } catch (...) {
        return handleAllExceptions(-1, ERR);
    }
}
```

When generating code, the CLib source generator uses the docstring of the original C++
code in combination with "crosswalks" of type information as described in
the [](sec-sourcegen-clib-details).

(sec-sourcegen-clib-install)=
## Building the Generated CLib Interface

Compilation of the generated CLib interface is fully integrated into the build
process, and is available after [building the main Cantera library](sec-compiling) with
default options. The CLib test suite is invoked by running:

```bash
scons test-clib
```

### CLib Code Generation

Source generation for the CLib interface is fully integrated into the build process.
Files used by the CLib API can be generated manually by running the following command
from the root folder of the Cantera source code:

```bash
sourcegen --api=clib --output=interfaces/clib
```

Generated files are placed in the output folder `interfaces/clib`, which is the
same as for the automated build process. Note that this step requires installation of
sourcegen via `python -m pip install -e interfaces/sourcegen`.

## CLib Source Generator Overview

The CLib source generator follows the generic layout of sourcegen's
[automated code generation](sec-sourcegen-details), with all code located in
the `interfaces/sourcegen/src/sourcegen/clib` folder. While the overall configuration
follows available [](sourcegen-config), the CLib source generator introduces additional
configuration options.

### Configuration

The YAML file `config.yaml` within the `clib` folder contains configuration options
specific to the CLib interface. Configuration options are static unless new CLib
modules need to be implemented (see section [](sec-sourcegen-clib-extend)).

- **Generic Options:** As the CLib interface follows directly from
  [](sec-sourcegen-specifications), override options defined by `ignore_files` and
  `ignore_funcs` are not needed. While the options are available, they should only be
  used for testing purposes and otherwise be left at their default values (empty).

- **Type Crosswalks:** These fields map C++ types to their CLib equivalents.

    - `ret_type_crosswalk`: Specifies the types returned by CLib functions and methods.
    - `par_type_crosswalk`: Specifies the types passed as parameters in CLib functions
      and methods.

- **CLib-Specific Options:** These fields define options specific to the CLib interface
  and use base class names defined within the C++ Cantera namespace. Functions defined
  within Cantera's root namespace use `""` as the class name:

    - `preambles`: A mapping of class names to associated text blocks for preambles
      (headers).
    - `includes`: A mapping of class names to lists of C++ includes defining base
      classes and associated specializations.

(sec-sourcegen-clib-details)=
### Implementation Details

- **Templates for Scaffolding:** Templates, powered by
  [Jinja](https://jinja.palletsprojects.com), are used to scaffold elements of the CLib
  API. The following files define these templates:

    - `templates.yaml`: Defines code blocks within the header and implementation files.
    - `template_header.h.in`: Defines the template for header files.
    - `template_source.cpp.in`: Defines the template for implementation files.

- **Source Code:** The implementation of the CLib source generator is contained in
  `generator.py`.

(sec-sourcegen-clib-extend)=
## Extending the CLib API

Sourcegen uses a one-to-one correspondence of YAML configuration files to C++ base
classes; derived classes are handled by the same configuration as the base class.

- **New Methods for Existing Configurations:** Add the name of the method as a new
  recipe; the new CLib function will become available once CLib is regenerated and
  Cantera is recompiled/reinstalled.

- **YAML Configuration for a C++ Base Class:** The CLib source generator implements
  templates for C++ interface patterns commonly used by Cantera.

  Follow the following steps for new classes and associated methods:

  1. Add a new YAML configuration file as described in [](sourcegen-config).
  1. Add include files specifying base class and specializations to the `includes`
     mapping in `config.yaml`.
  1. Regenerate the CLib interface and recompile/reinstall Cantera.
  1. Add new unit tests in `test/clib_generated` to ensure that the new feature is
     working properly.

## Troubleshooting

The **sourcegen** utility uses a logging module to provide feedback. Add the verbose
`-v` option to generate additional feedback.

- *Missing XML tree:* The sourcegen utility requires a valid Doxygen tag file and an
  associated XML tree, which are generated by running `scons doxygen`.

  ```shell
  [CRITICAL] Tag file does not exist at expected location:
      <...>/cantera/build/doc/Cantera.tag
  Run 'scons doxygen' to generate.
  ```

- *Invalid function/method name:* The function/method name is not known to Doxygen;
  check spelling and/or re-run `scons doxygen` if the function/method was recently
  created.

  ```shell
  [CRITICAL] Could not find '...' in Doxygen tag file.
  ```

- *Missing docstring:* Cantera's Doxygen configuration skips undocumented functions and
  methods; thus, they are not part of the XML tree and cannot be resolved by sourcegen.

  ```shell
  [CRITICAL] Unable to resolve recipe type for '...'
  ```

- *Ambiguous Signature:* Functions and methods that have overloads and/or define default
  arguments require disambiguation via the `wraps` field.

  ```shell
  [CRITICAL] Need argument list to disambiguate '...'.
  Possible matches are:
   - (double, double, const double*)
   - (double, double, const Composition&)
   - (double, double, const string&)
  ```

- *Missing Crosswalk:* The CLib code generator is limited to type crosswalks
  defined `config.yaml`.

  ```shell
  [CRITICAL] Failed crosswalk for argument type '...'.
  [CRITICAL] Failed crosswalk for return type '...'.
  ```

  A resolution will require one of the two options:

  1. Create alternative C++ functions/methods with signatures that are compatible with
     existing crosswalks.
  1. Add new crosswalks to `config.yaml`, which may require updates of templates as
     well as the CLib source generator source code itself.
