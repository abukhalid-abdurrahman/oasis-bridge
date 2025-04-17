"use client";

import { chartExample } from "@/lib/helpers/chartExample";
import { AreaSeries, createChart, LineStyle } from "lightweight-charts";
import { useEffect, useRef } from "react";

export default function Chart({ className }: { className?: string }) {
  const chartContainerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const chart = createChart(chartContainerRef.current!, {
      layout: {
        attributionLogo: false,
        background: {
          type: "solid" as any,
          color: "transparent",
        },
        textColor: "#d1d4dc",
      },
      grid: {
        vertLines: {
          color: "rgba(255, 255, 255, 0.15)",
          style: LineStyle.LargeDashed,
        },
        horzLines: {
          color: "rgba(255, 255, 255, 0.15)",
          style: LineStyle.LargeDashed
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
    <div className={`overflow-hidden px-2 rounded-2xl ${className}`}>
      <div
        className="w-full h-full flex justify-center items-center lg:h-96"
        ref={chartContainerRef}
      />
    </div>
  );
}
