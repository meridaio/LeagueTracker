module internal Colors

open System.Drawing
open MagicHome

let colorToRgb (color: Color) =
    RGB(color.R, color.G, color.B)
