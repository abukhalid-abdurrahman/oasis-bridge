"use client";

import { format } from "date-fns";
import { AreaSeries, createChart, LineStyle } from "lightweight-charts";
import { useEffect, useMemo, useRef } from "react";

interface ChartProps {
  firstData: any;
  data: any;
  className?: string;
}

export default function Chart({ className, data, firstData }: ChartProps) {
  const chartContainerRef = useRef<HTMLDivElement>(null);
  const convertedData = useMemo(() => {
    const allData = [{...firstData}, ...data];
    return allData.map((item: any) => {
      const date = item?.createdAt || item?.changedAt;
      const price = item?.price || item?.newPrice
      return {
        time: format(new Date(date), "yyyy-MM-dd"),
        value: price,
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
    </div>
  );
}
