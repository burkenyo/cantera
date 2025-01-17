/**
 *  @file ReactionRateFactory.h
 *  Factory class for reaction rate objects. Used by classes that implement kinetics
 *  (see @ref reactionGroup and class @link Cantera::ReactionRate ReactionRate@endlink).
 */

// This file is part of Cantera. See License.txt in the top-level directory or
// at https://cantera.org/license.txt for license and copyright information.

#ifndef CT_NEWRATE_H
#define CT_NEWRATE_H

#include "cantera/base/FactoryBase.h"
#include "cantera/kinetics/ReactionRate.h"

namespace Cantera
{

class Kinetics;
class Units;

/**
 * @defgroup arrheniusGroup Arrhenius-type Parameterizations
 * Classes implementing the standard Arrhenius rate parameterization and derived models.
 * @ingroup reactionGroup
 */

/**
 * @defgroup falloffGroup Falloff Parameterizations
 * Classes implementing fall-off in reaction rate constants due to intermolecular energy
 * transfer and derived models.
 * @ingroup reactionGroup
 */

/**
 * @defgroup surfaceGroup Interface Rate Parameterizations
 * Classes implementing reaction rates that involve interfaces.
 * @ingroup reactionGroup
 */

/**
 * @defgroup otherRateGroup Other Reaction Rate Parameterizations
 * Classes implementing other reaction rate parameterizations.
 * @ingroup reactionGroup
 */


/**
 * Factory class to construct reaction rate calculators.
 * The reaction factory is accessed through the static method factory:
 *
 * @code
 * Rate* f = ReactionRateFactory::factory()->newReactionRate(type, c)
 * @endcode
 */
class ReactionRateFactory
    : public Factory<ReactionRate, const AnyMap&, const UnitStack&>
{
public:
    /**
     * Return a pointer to the factory. On the first call, a new instance is
     * created. Since there is no need to instantiate more than one factory,
     * on all subsequent calls, a pointer to the existing factory is returned.
     */
    static ReactionRateFactory* factory();

    virtual void deleteFactory();

private:
    //! Pointer to the single instance of the factory
    static ReactionRateFactory* s_factory;

    //! default constructor, which is defined as private
    ReactionRateFactory();

    //!  Mutex for use when calling the factory
    static std::mutex rate_mutex;
};

//! @addtogroup reactionGroup
//! @{

//! Create a new empty ReactionRate object
/*!
 * @param type string identifying type of reaction rate.
 */
shared_ptr<ReactionRate> newReactionRate(const string& type);

//! Create a new Rate object using the specified parameters
/*!
 * @param rate_node AnyMap node describing reaction rate.
 * @param rate_units Vector describing unit system of the reaction rate; each element
 *          specifies Unit and exponent applied to the unit.
 */
shared_ptr<ReactionRate> newReactionRate(
    const AnyMap& rate_node, const UnitStack& rate_units);

//! Create a new Rate object using the specified parameters
/*!
 * @param rate_node AnyMap node describing reaction rate.
 */
shared_ptr<ReactionRate> newReactionRate(const AnyMap& rate_node);

//! @}

}
#endif
