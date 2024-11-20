from manim import * 
import pandas as pd

df = pd.read_excel("fy25-adc-high-school-data.xlsx", sheet_name="1")  # Replace with actual file name if needed
B_n_values = df['B_n'].to_list()
int_B_n_values = [int(x) for x in B_n_values]


class rectangle(Scene):
    def construct(self):
        for i in range(10):
            k=0
            rect = always_redraw(Rectangle(width=.2, height=(B_n_values[k]/1000000)).to_edge(UL))
            k+=1
            self.play(Create(rect))



from manim import * 
import pandas as pd

# Load data from Excel
df = pd.read_excel("fy25-adc-high-school-data.xlsx", sheet_name="1")  # Replace with actual file name if needed
B_n_values = df['B_n'].to_list()

class rectangle(Scene):
    def construct(self):
        for i in range(10):
            # Create a new rectangle each time with the i-th value from B_n_values
            rect = Rectangle(width=0.2, height=B_n_values[i] / 1000000).to_edge(UL)
            
            # Play animation to create the rectangle
            self.play(Create(rect))
