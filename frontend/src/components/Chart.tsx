"use client";

import { chartExample } from "@/lib/helpers/chartExample";
import { AreaSeries, createChart } from "lightweight-charts";
import { useEffect, useRef } from "react";

export default function Chart() {
  const chartContainerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const chart = createChart(chartContainerRef.current!, {
      width: 750,
      height: 400,
      layout: {
        attributionLogo: false,
        background: {
          type: "solid" as any,
          color: "#131722",
        },
        textColor: "#d1d4dc",
      },
      grid: {
        vertLines: {
          color: "rgba(42, 46, 57, 0)",
        },
        horzLines: {
          color: "rgba(42, 46, 57, 0.6)",
        },
      },
    });

    const candleSeries = chart.addSeries(AreaSeries, {
      lineColor: "#2962FF",
      topColor: "rgba(12, 18, 59, 0.6)",
      bottomColor: "rgba(12, 18, 59, 0.1)",
    });

    candleSeries.setData(chartExample);

    chart.timeScale().fitContent();

    return () => chart.remove();
  }, []);

  return (
    <div
      className="bg-[#131722] w-[780px] h-[430px] flex justify-center items-center rounded-2xl"
      ref={chartContainerRef}
    />
  );
}
