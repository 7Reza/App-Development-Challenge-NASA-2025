import pandas as pd
import math

# Load the Excel file
df = pd.read_excel("fy25-adc-high-school-data.xlsx", sheet_name="1")  # Replace with your sheet name if necessary

# Define the function to calculate B_n for a given R
def LinkBudgetEquation(R):
    # Constants
    P = 10  # dBW Satellite Transmitter Power
    G = 9  # dBi Satellite Antenna Gain
    Losses = 19.43  # dB Losses
    N_r = 0.55  # Ground Station Antenna Efficiency
    λ = 0.136363636  # m Wavelength
    k_b = -228.6  # dB Boltzmann constant
    T_s = 22  # K System Noise Temperature

    # Calculate the terms inside the exponent
    term1 = P + G - Losses
    term2 = 10 * math.log10(N_r * ((math.pi * 34) / λ) ** 2)
    term3 = -20 * math.log10((4000 * math.pi * R) / λ)
    term4 = -k_b
    term5 = -10 * math.log10(T_s)
    
    exponent = 0.1 * (term1 + term2 + term3 + term4 + term5)
    
    # Calculate B_n
    B_n = (10 ** exponent) / 1000

    return B_n

# Apply the function to the R(km) column and store results in a new column 'B_n'
df['B_n'] = df['R(km)'].apply(LinkBudgetEquation)

# Save the updated DataFrame back to the Excel file
df.to_excel("fy25-adc-high-school-data.xlsx", sheet_name="1", index=False)
print("Updated Excel file with B_n column.")
