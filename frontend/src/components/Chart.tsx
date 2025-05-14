"use client";

import { chartExample } from "@/lib/helpers/chartExample";
import { format } from "date-fns";
import { AreaSeries, createChart, LineStyle } from "lightweight-charts";
import { useEffect, useMemo, useRef } from "react";

interface ChartProps {
  className?: string;
  data: any;
}

export default function Chart({ className, data }: ChartProps) {
  const chartContainerRef = useRef<HTMLDivElement>(null);
  const convertedData = useMemo(() => {
    return data.map((item: any) => {
      return {
        time: format(new Date(item.changedAt), "yyyy-MM-dd"),
        value: item.newPrice,
      };
    });
  }, [data]);

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
          style: LineStyle.LargeDashed,
        },
      },
    });

    const candleSeries = chart.addSeries(AreaSeries, {
      lineColor: "#2962FF",
      topColor: "rgba(12, 18, 59, 0.6)",
      bottomColor: "rgba(12, 18, 59, 0.1)",
    });

    candleSeries.setData(convertedData);

    chart.timeScale().fitContent();

    return () => chart.remove();
  }, []);

  return (
    <div className={`overflow-hidden px-2 rounded-2xl relative ${className}`}>
      <div
        className="w-full h-full flex justify-center items-center lg:h-96"
        ref={chartContainerRef}
      />
      {data.length <= 0 && (
        <div className="absolute top-0 left-0 right-0 bottom-0 bg-neutral-900 w-full h-full flex justify-center items-center z-[1000000] p-5">
          <h3 className="h3 text-textGray text-lg">
            There have been no changes in the RVA price yet
          </h3>
        </div>
      )}
    </div>
  );
}
