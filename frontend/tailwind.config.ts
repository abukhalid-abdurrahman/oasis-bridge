import type { Config } from "tailwindcss";

export default {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        background: "var(--background)",
        gray: "var(--gray)",
        darkGray: "var(--dark-gray)",
        textGray: "var(--text-gray)",
      },
      screens: {
        'xl': {'max': '1280px'},
        'lg': {'max': '1024px'},
        'md': {'max': '768px'},
        'sm': {'max': '640px'},
        'xs': {'max': '480px'},
        'xxs': {'max': '400px'},
      }
    },
  },
  plugins: [],
} satisfies Config;
