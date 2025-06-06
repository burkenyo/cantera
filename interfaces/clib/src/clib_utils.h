/**
 * @file clib_utils.h
 */

// This file is part of Cantera. See License.txt in the top-level directory or
// at https://cantera.org/license.txt for license and copyright information.

#ifndef __CLIB_UTILS_H__
#define __CLIB_UTILS_H__

#include "cantera/base/ctexceptions.h"
#include "cantera_clib/clib_defs.h"

#include "application.h"
#include "../clib/Cabinet.h"  // File needs to be moved
#include <iostream>


namespace Cantera
{

//! Exception handler used at language interface boundaries.
/*!
 * When called from a "catch (...)" block, this function will attempt to save
 * an error message in global error stack and return a value indicating the
 * type of exception caught.
 *
 * @param ctErrorCode Value to return if a CanteraError is caught
 * @param otherErrorCode Value to return if a different exception is caught
 */
template <typename T>
T handleAllExceptions(T ctErrorCode, T otherErrorCode)
{
    // Rethrow the previous exception, then catch a more
    // specific exception type if possible.
    try {
        throw;
    } catch (CanteraError& cterr) {
        Application::Instance()->addError(cterr.what());
        return ctErrorCode;
    } catch (std::exception& err) {
        std::cerr << "Cantera: caught an instance of "
                  << err.what() << std::endl;
        Application::Instance()->addError(err.what());
        return otherErrorCode;
    } catch (...) {
        std::cerr << "Cantera: caught an instance of "
                  "an unknown exception type" << std::endl;
        Application::Instance()->addError("unknown C++ exception");
        return otherErrorCode;
    }
}

}

#endif
