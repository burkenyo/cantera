"""
Compute the "equilibrium" and "frozen" sound speeds for a gas

Requires: cantera >= 2.6.0
Keywords: thermodynamics, equilibrium
"""

import cantera.with_units as ct
import numpy as np

ct.units.default_format = ".2F~P"

def equilibrium_sound_speeds(gas, rtol=1.0e-6, max_iter=5000):
    """
    Returns a tuple containing the equilibrium and frozen sound speeds for a
    gas with an equilibrium composition.  The gas is first set to an
    equilibrium state at the temperature and pressure of the gas, since
    otherwise the equilibrium sound speed is not defined.
    """

    # set the gas to equilibrium at its current T and P
    gas.equilibrate('TP', rtol=rtol, max_iter=max_iter)

    # save properties
    s0 = gas.s
    p0 = gas.P
    r0 = gas.density

    # perturb the pressure
    p1 = p0*1.0001

    # set the gas to a state with the same entropy and composition but
    # the perturbed pressure
    gas.SP = s0, p1

    # frozen sound speed
    afrozen = np.sqrt((p1 - p0)/(gas.density - r0)).to("ft/s")

    # now equilibrate the gas holding S and P constant
    gas.equilibrate('SP', rtol=rtol, max_iter=max_iter)

    # equilibrium sound speed
    aequil = np.sqrt((p1 - p0)/(gas.density - r0)).to("ft/s")

    # compute the frozen sound speed using the ideal gas expression as a check
    gamma = gas.cp/gas.cv
    gamma * ct.units.molar_gas_constant
    afrozen2 = np.sqrt(gamma * ct.units.molar_gas_constant * gas.T /
                         gas.mean_molecular_weight).to("ft/s")

    return aequil, afrozen, afrozen2

# test program
if __name__ == "__main__":
    gas = ct.Solution('gri30.yaml')
    gas.X = 'CH4:1.00, O2:2.0, N2:7.52'
    T_range = np.linspace(80.33, 4760.33, 50) * ct.units.degF
    print("Temperature      Equilibrium Sound Speed     Frozen Sound Speed      Frozen Sound Speed Check")
    for T in T_range:
        gas.TP = T, 1.0 * ct.units.atm
        print(T, *equilibrium_sound_speeds(gas), sep = "               ")