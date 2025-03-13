from manim import *
import pandas as pd

# Load data from the Excel file
df = pd.read_excel("fy25-adc-high-school-data.xlsx", sheet_name="1")  # Replace with actual file name if needed
R_values = df['T'].to_list()
B_n_values = df['B_n'].to_list()


# Graphing B_n over R
class PlotGraph(Scene):
    def construct(self):

        # Create the equation with the \[ ... \] environment (no numbering)
        eq = Tex(R"""\[
        B_n=\frac{10^{\frac{1}{10}\left[P_t+G_t-\text { Losses }+10 \log _{10} \eta_R\left(\frac{\pi 34}{\lambda}\right)^2-20 \log _{10} \frac{4000 \pi R}{\lambda}-k_b-10 \log _{10} T_s\right]}}{1000}
        \]""").scale(0.3).to_edge(UL)

        # Show the equation
        self.play(Write(eq), run_time=3)

        # Hold on screen
        self.wait(2)

        # Set up axes for the graph
        axes = Axes(
            x_range=[0, max(R_values), max(R_values)//10],  # Adjust range based on R data
            y_range=[0, max(B_n_values), max(B_n_values)//10],  # Adjust range based on B_n data
            x_length=10,
            y_length=6,
            axis_config={"color": BLUE},
            x_axis_config={"include_numbers": True, "label_direction": DOWN},
            y_axis_config={"include_numbers": True, "label_direction": LEFT}
        ).scale(.8)

        # Label axes
        x_label = axes.get_x_axis_label("R (km)").scale(.8)
        y_label = axes.get_y_axis_label("B_n (kbps)").scale(.8)

        # Create line plot
        graph = axes.plot_line_graph(
            x_values=R_values,
            y_values=B_n_values,
            add_vertex_dots=True,
            line_color=YELLOW
        ).scale(.8)

        # Add graph and labels to the scene
        self.play(Create(axes), Write(x_label), Write(y_label))
        self.play(Create(graph))
        self.wait(3)
