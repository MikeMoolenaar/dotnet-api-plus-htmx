/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["../views/*.liquid"],
    plugins: [
        require('tailwindcss'),
        require('autoprefixer'),
        require("daisyui")
    ],
    daisyui: {
        themes: ["light", "night"],
        darkTheme: "night"
    }
}