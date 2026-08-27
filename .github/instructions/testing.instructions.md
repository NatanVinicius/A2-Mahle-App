# Testing Instructions

## Hardware

The physical Keyence IV4 hardware may not be available during development.

When hardware is unavailable, use the Fake implementation.

## Fake Sensor

The Fake must simulate the same Application contract as the real sensor.

Tests and development flows must not require the Client to know whether the sensor is Fake or real.

## Validation Priority

When implementing a feature:

1. Compile the solution.
2. Validate dependency injection.
3. Validate the application flow.
4. Validate using the Fake when hardware is unavailable.
5. Test real hardware integration only when the hardware is available.

## Do Not Fake Application Behavior

The Fake should simulate external input.

It should not bypass:

- correlation;
- application services;
- domain models;
- persistence;
- UI flow.

The goal is to exercise the same application pipeline that will be used with the real Keyence implementation.