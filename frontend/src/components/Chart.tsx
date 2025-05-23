"use client";

import { AreaSeries, createChart, LineStyle } from "lightweight-charts";
import { useEffect, useMemo, useRef } from "react";

interface ChartProps {
  firstData: any;
  data: any;
  className?: string;
}

export default function Chart({ className, data, firstData }: ChartProps) {
  const chartContainerRef = useRef<HTMLDivElement>(null);
  const firstChangedData = useMemo(() => {
    return {
      time: new Date(firstData.createdAt).getTime() / 1000,
      value: data?.[0]?.oldPrice,
    };
  }, [data, firstData]);

  const convertedData = useMemo(() => {
    if (!data || data.length === 0) {
      const date = firstData?.createdAt || firstData?.changedAt;
      const price = firstData?.price || firstData?.newPrice;

      return [
        {
          time: new Date(date).getTime() / 1000,
          value: price,
        },
      ];
    } else {
      const allData = [...data];
      const result = allData.map((item: any) => {
        const date = item?.createdAt || item?.changedAt;
        const price = item?.price || item?.newPrice;
        return {
          time: new Date(date).getTime() / 1000,
          value: price,
        };
      });

      result.unshift(firstChangedData);
      return result;
    }
  }, [data, firstData, firstChangedData]);

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

    candleSeries.setData(convertedData as any);

    chart.timeScale().fitContent();
    console.log(convertedData)
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
