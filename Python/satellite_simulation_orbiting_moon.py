import pandas as pd
import matplotlib.pyplot as plt  
import numpy as np
import math

df = pd.read_csv('position_data_moon_with_time.csv')

k = len(df)

# Calculating the radius of satellite-moon orbit
gravitational_constant = 6.67430e-11  # m^3 kg^-1 s^-2
moon_mass = 7.342e22  # kg
velocity = 1600  # m/s
angular_velocity_radians = velocity / r  # v/r gives radians per second
angular_velocity_degrees = math.degrees(angular_velocity_radians) # degrees per second

def radius(v):
    radius = (gravitational_constant * moon_mass) / (v ** 2)
    return radius   

# Calculate radius in meters
r = radius(velocity)  # Keep in meters for calculations
r_km = r / 1000  # Convert to kilometers for reference
print(f"Orbital radius: {r_km:.2f} km")


def xyz_position_satellites(df):
    list_time_differences = []
    x_pos_s1, y_pos_s1, z_pos_s1 = [], [], []
    x_pos_s2, y_pos_s2, z_pos_s2 = [], [], []
    x_pos_s3, y_pos_s3, z_pos_s3 = [], [], []
    x_pos_s4, y_pos_s4, z_pos_s4 = [], [], []
    x_pos_s5, y_pos_s5, z_pos_s5 = [], [], []

    for i in range(k - 1):
        time_difference = df.iloc[i + 1, 0] - df.iloc[i, 0] 
        list_time_differences.append(time_difference)

    for i in range(k - 1):
        theta_s1 = angular_velocity_degrees * list_time_differences[i]
        theta_s2 = angular_velocity_degrees * list_time_differences[i] + 72
        theta_s3 = angular_velocity_degrees * list_time_differences[i] + 144
        theta_s4 = angular_velocity_degrees * list_time_differences[i] + 216
        theta_s5 = angular_velocity_degrees * list_time_differences[i] + 288

        # Calculate positions for satellite #1
        x_s1 = df.iloc[i, 1] + r * np.sin(theta_s1)
        y_s1 = df.iloc[i, 2] + r * np.cos(theta_s1)
        z_s1 = df.iloc[i, 3]
        x_pos_s1.append(x_s1)
        y_pos_s1.append(y_s1)
        z_pos_s1.append(z_s1)

        # Calculate positions for satellite #2
        x_s2 = df.iloc[i, 1] + r * np.sin(theta_s2)
        y_s2 = df.iloc[i, 2] + r * np.cos(theta_s2)
        z_s2 = df.iloc[i, 3]
        x_pos_s2.append(x_s2)
        y_pos_s2.append(y_s2)
        z_pos_s2.append(z_s2)

        # Calculate positions for satellite #3
        x_s3 = df.iloc[i, 1] + r * np.sin(theta_s3)
        y_s3 = df.iloc[i, 2] + r * np.cos(theta_s3)
        z_s3 = df.iloc[i, 3]
        x_pos_s3.append(x_s3)
        y_pos_s3.append(y_s3)
        z_pos_s3.append(z_s3)

        # Calculate positions for satellite #4
        x_s4 = df.iloc[i, 1] + r * np.sin(theta_s4)
        y_s4 = df.iloc[i, 2] + r * np.cos(theta_s4)
        z_s4 = df.iloc[i, 3]
        x_pos_s4.append(x_s4)
        y_pos_s4.append(y_s4)
        z_pos_s4.append(z_s4)

        # Calculate positions for satellite #5
        x_s5 = df.iloc[i, 1] + r * np.sin(theta_s5)
        y_s5 = df.iloc[i, 2] + r * np.cos(theta_s5)
        z_s5 = df.iloc[i, 3]
        x_pos_s5.append(x_s5)
        y_pos_s5.append(y_s5)
        z_pos_s5.append(z_s5)

    return (
        x_pos_s1, y_pos_s1, z_pos_s1,
        x_pos_s2, y_pos_s2, z_pos_s2,
        x_pos_s3, y_pos_s3, z_pos_s3,
        x_pos_s4, y_pos_s4, z_pos_s4,
        x_pos_s5, y_pos_s5, z_pos_s5
    )

# Call the function to get the positions
(
    x_pos_s1, y_pos_s1, z_pos_s1,
    x_pos_s2, y_pos_s2, z_pos_s2,
    x_pos_s3, y_pos_s3, z_pos_s3,
    x_pos_s4, y_pos_s4, z_pos_s4,
    x_pos_s5, y_pos_s5, z_pos_s5
) = xyz_position_satellites(df)

# Create DataFrame and save to CSV
result_df = pd.DataFrame({
    'x_pos_s1': np.round(x_pos_s1, 2),  # Round to 2 decimal places
    'y_pos_s1': np.round(y_pos_s1, 2),
    'z_pos_s1': np.round(z_pos_s1, 2),
    'x_pos_s2': np.round(x_pos_s2, 2),
    'y_pos_s2': np.round(y_pos_s2, 2),
    'z_pos_s2': np.round(z_pos_s2, 2),
    'x_pos_s3': np.round(x_pos_s3, 2),
    'y_pos_s3': np.round(y_pos_s3, 2),
    'z_pos_s3': np.round(z_pos_s3, 2),
    'x_pos_s4': np.round(x_pos_s4, 2),
    'y_pos_s4': np.round(y_pos_s4, 2),
    'z_pos_s4': np.round(z_pos_s4, 2),
    'x_pos_s5': np.round(x_pos_s5, 2),
    'y_pos_s5': np.round(y_pos_s5, 2),
    'z_pos_s5': np.round(z_pos_s5, 2)
})

result_df.to_csv('5_stellite_position.csv', index=True)

