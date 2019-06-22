# Electric Microgrid Particle Swarm Optimizer

Uses Binary Particle Swarm Optimization (BPSO) to solve the Unit Commitment Problem in the context of electric power generation in an idealized microgrid.

Features:
- Good performance BPSO implementation with bitwise operations.
- Premature convergence detection and correction in the form of particle craziness.
- Running multiple BPSOs in parallel on different threads (unavailable in WebGL).
- Takes into consideration realtime market prices.
- Snapshot system allows visualizing the BPSO algorithm's functioning at each iteration.
- Visualizations of the microgrid's state.
- Ability to customize data such as load curve and generator specifications.
- Importing and exporting of data as .csv files.

## Table of Contents
- [Unit Commitment Problem Statement](#unit-commitment-problem-statement)
- [Microgrid Model](#microgrid-model)
  - [Microgrid State](#microgrid-state)
  - [Battery](#battery)
  - [Generators](#generators)
  - [Simplifications compared to reality](#simplifications)
- [Operation](#operation)
- [BPSO](#bpso)
- [PSO Visualizations](#pso-visualizations)
  - [Radar](#radar)
  - [Schedule Map](#schedule-map)
  - [Convergence](#convergence)

## Unit Commitment Problem Statement

Given a microgrid that:
- Is connected to the main grid where energy can be imported or exported at market prices.
- Has a local demand curve that needs to be satisfied.
- Has multiple heterogenous generators with operating costs and minimum uptime and minimum downtime constraints.

Find the schedule (on/off states at each time step) for the generators that will lead to the minimum costs / maximum profit for the grid over a period of 24h.

The Big O complexity of an exhaustive search is `O(2^(n*t))` where `n` is the number of generators and `t` is the number of time steps. For 3 generators and 24 time steps, the number of possible configuration is ~4.7 sexdecillions. Another way to think about it is that it exceeds the storage capacity of an Int32 by roughly 1 trillion times. Many of these states are invalid because they violate the minimum uptime and minimum downtime constraints, which effectively define how quickly a generator can be toggled.

## Microgrid Model

The microgrid is modelled as having idealized:
- Renewable energy sources. Wind and solar power.
- Battery system connected to the renewable energy sources.
- Point of common coupling (PCC) with the main grid.
- Heterogenous thermal generators that can be switched on and off.

### Microgrid State
The state of the microgrid is composed of the following variables:
- A local power demand curve. Measured in MW.
- Wind and solar power generation curves. Measured in MW.
- Battery energy curve. Represents how much energy is currently stored in the battery system in MWh.
- Battery state of charge curve. Represents how much energy stored in the battery system as a fraction of maximum energy storage.
- Energy exchange curve. Represents energy exchange in MWh with the main grid. Positive values are imports and negative values are exports.
- Generator schedules, one for each generator. Defines the on and off states of the generators at each point in time.
- Generator production curves, one for each generator. Defines how much power a generator is outputting at each point in time.

### Battery
The battery system is modelled as having:
- A total storage capacity measured in MWh.
- A maximum power it can export or import measured in MW.
- An initial state of charge (SoC). The SoC the battery starts with at t = 0.
- A minimum and maximum SoC.

### Generators
The thermal generators are modelled as having:
- A maximum power they can generate measured in MW.
- Cost coefficients a, b, c that define the curve of the quadratic cost equation a*P^2 + b*P + c.
- Minimum uptime. The amount of minutes the generator has to stay on before being free to be switched off.
- Minimum downtime. The amount of minutes the generator has to stay off before being free to be switched on.

### Simplifications
- Generators can vary their power output instantly, by any amount, as long as they're already on.
- All generations have a minimum power output of zero.
- Prices are known for the entire day, but buying and selling is done as if on a realtime market. This best resembles working based off of a realtime market 24h prediction.
- Buying and selling is done instantaneously and with a granularity of one minute. In reality, realtime markets have bids and the exchange needs to be constant over a longer interval of time.
- Reactive power considerations are ignored.
- Excess renewable power is ignored as if renewable sources could instantly reduce their output or dump it.

## Operation

1. Renewable energy is used first. Excess renewable energy is stored in the battery. Conversely shortages are made up by drawing power from the battery. 
2. Any remaining demand has to be met either with electricity from the thermal generators or from imports.
3. The state of the grid, market prices and remaining local demand are fed into the Unit Commitment PSO algorithm that outputs the closest-to-optimal generator schedules it could find.
4. The generator schedules are fed into an algorithm that solves Economic Dispatch. The goal of this algorithm is to balance the load between all the online generators in a way that maximizes profit. The algorithm outputs the best generator production curves it could find.
5. The generator production curves are used to determine the full state of the microgrid, computing total cost and energy exchange.

## BPSO

The search space is modelled as having one time dimension and n binary dimensions (one for each thermal generator). As a result, a position in this search space is represented as an array of binary integers where each row of binary integers corresponds to the on/off states of the generators at that particular time step. 

``p[10, 52]`` => ``00110100`` => generators 0, 1, 4, 6, 7 are off and generators 2, 3, 5 are on.

The solution to the UC problem is a position that has the smallest cost. This cost is used as the fitness associated to a position.

Particles have:
- **A position ``p``**.
- **A velocity ``v``**.
- The fitness of the best position they've found so far **``PBestFitness``**.
- The best position they've found so far **``PBest``**.

Additionally the swarm stores:
- The fitness of the global best position **``GBestFitness``**.
- The best position found by the entire swarm so far **``GBest``**.

The BPSO algorithm utilizes a simple **PBest/GBest topology** where each particle tracks their personal best, the swarm tracks the global best and particles are attracted both towards their PBest and the swarm's GBest. 

At every iteration a new velocity is calculated for the particles. The new position is obtained by XORing the current position with the new velocity.

In the velocity calculation a few factors come into play:
- There's an **inertial probability ``w``** to take the previous velocity into consideration by ORing it to the rest of the equation. This approximates how continuous space inertia works in a binary space.
- A random amount of bits are switched to move the particle towards to PBest.
- A random amount of bits are switched to move the particle towards to GBest.
- There's a **craziness probability ``c``** to XOR random noise into the velocity calculation. ``c`` goes up with each iteration that hasn't lead to a GBest update.

Notice that since we're in a binary space the magnitude of velocity is represented as a random bitmask that controls how many bits switch at a time. The particle doesn't instantly move to PBest or GBest because a random bitmask forces the change to happen to only some bits every iteration.

## PSO Visualizations

### Radar

![Radar Screenshot](https://github.com/Anvoker/MicrogridPSO/blob/master/gh/radars.png)

Allows us to visualize the convergence of the algorithm by showing the amount of particles whose current position or PBest defines a generator has being on or off at a certain time step.

24 Radars are rendered, one for each hour of the day.

A radar has one axis for every generator.

The value along that axis represents the amount of particles that "think" the generator should be on during that hour.

A slider controls which iteration is being currently viewed. By using the slider we can see how the positions change over time.

### Schedule Map
![Schedule Map](https://github.com/Anvoker/MicrogridPSO/blob/master/gh/schedulemap.png)

Best used to visualize changes in the generator schedule from iteration to iteration.

The schedule map has a series of rows, one for each iteration.

A schedule row contains one schedule cell for each generator.

A schedule cell is a barcode image that represents the on off states of a generator over a 24h period. If a colored bar is drawn at a certain hour, then the generator is on. If the space is empty (white) then the generator is off.

### Convergence

Simple 2D graph of best fitness over time. Very good for identifying when the swarm is trapped in a local minima.

## Building
Requires Unity 2019.x and Unity 2019.3.0a5 or later for multithreaded WebGL only.

Proprietary dependencies:
- Vectrosity 5. Must be obtained separately. Used for drawing visualizations.

OSS dependencies included in the repository:
- TextMeshPro.
- StandaloneFileBrowser. Thanks Gökhan Gökçe!
- fgCSVReader. Thanks Frozax!
- EasyButtons. Thanks Mads Bang Hoffensetz!
- Simple Windows Manager. Thanks intCode Studio!
- StringFormatter. Thanks Michael Popoloski!
- System.Runtime.CompilerServices.Unsafe.

### Roadmap
- Replace Vectrosity 5 with a OSS line drawing library.
- Add minimum power constraint to thermal generators.
- Implement probabilistic visualisation of high-dimensional binary data.
- Add Parallel coordinates plot for PSO visualization.
- Introduce better premature convergence mitigation.
- Reintroduce heuristic adjustments.
