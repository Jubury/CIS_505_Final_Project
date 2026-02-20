# Autonomous Lane Changing System
A rule-based autonomous driving system that performs safe lane changes in real-time using Time-to-Collision (TTC) calculations and dynamic safety thresholds.

## Overview
This system enables autonomous vehicles to make intelligent lane-change decisions in highway scenarios while maintaining safety through collision avoidance mechanisms. Built in C# using the Godot Engine, it executes at 60 Hz (<0.06ms per frame) to ensure real-time responsiveness.

## Key Features
- **Real-time Decision Making**: Executes lane change logic at 60 Hz for immediate response to traffic conditions
- **Time-to-Collision Safety**: Calculates TTC for both front and rear vehicles to prevent unsafe merges (minimum 3-second threshold)
- **Weather Adaptation**: Adjusts safety thresholds dynamically based on weather conditions (rain, fog)
- **Mid-Merge Abort**: Monitors rear vehicles during lane changes and aborts if a faster vehicle approaches
- **RayCast Perception**: Uses ray-based detection to identify vehicles and lane boundaries

## Technical Implementation
Language**: C#  
Engine**: Godot 4  
Control System**: Rule-based decision logic with coroutine-driven execution

### Core Components
1. **Lane Safety Assessment**: Evaluates target lane using multiple raycasts to detect obstacles
2. **TTC Calculator**: Computes time-to-collision for surrounding vehicles based on relative speeds
3. **Merge Buffer System**: Requires lane to be clear for 1.5 seconds before initiating merge
4. **Fail-Safe Mechanism**: Continuously monitors rear threats and aborts merge if TTC drops below threshold

### Decision Flow
1. Check if car in front is slower than speed limit
2. Verify lane change cooldown has elapsed
3. Assess target lane safety using raycasts
4. Calculate front and rear vehicle TTC
5. Apply weather-based threshold adjustments
6. Execute merge with continuous monitoring
7. Abort if unsafe conditions detected mid-merge


## Performance
- Execution time: <0.06ms per frame (60 Hz)
- Successfully prevents unsafe merges in dynamic traffic scenarios
- Adapts to adverse weather conditions (rain, fog)
- Handles mid-merge threats with abort mechanism

## Simulation Testing
The system was validated in simulated highway environments with:
- Multi-lane traffic scenarios
- Variable vehicle speeds
- Weather condition changes
- Emergency abort situations

## Future Enhancements
- Integration with trajectory prediction for surrounding vehicles
- Expansion of detection arc for approaching threats
- Connection to global route planning systems
