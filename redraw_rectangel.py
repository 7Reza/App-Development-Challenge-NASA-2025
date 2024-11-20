from manim import *
import pandas as pd

# Load data from Excel
df = pd.read_excel("fy25-adc-high-school-data.xlsx", sheet_name="1")  # Replace with actual file name if needed
B_n_values = df['B_n'].to_list()

class rectangle(Scene):
    def construct(self):
        # Initialize the rectangle with the first height value
        rect = Rectangle(width=0.2, height=B_n_values[0] / 1000000).to_edge(UL)

        # Play the initial rectangle creation
        self.play(Create(rect))

        # Update the rectangle in the loop
        for i in range(1, 10):
            # Animate the update of the rectangle's height
            self.play(rect.animate.set_height(B_n_values[i] / 1000000))
