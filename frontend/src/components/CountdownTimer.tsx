"use client";

import { useEffect } from "react";

interface CountdownTimerProps {
  timeLeft: number;
  setTimeLeft: (callback: (prevTime: number) => number) => void;
}

export default function CountdownTimer({ timeLeft, setTimeLeft }: CountdownTimerProps) {
  useEffect(() => {
    if (timeLeft <= 0) return;

    const timer = setInterval(() => {
      setTimeLeft((prevTime: number) => {
        if (prevTime <= 1) {
          clearInterval(timer);
          return 0;
        }
        return prevTime - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [timeLeft]);

  const minutes = Math.floor(timeLeft / 60);
  const seconds = timeLeft % 60;

  return (
    <div className="items-center absolute bg-gray py-1 text-center rounded-full w-14">
      <p className="p-sm">
        {minutes}:{seconds.toString().padStart(2, "0")}
      </p>
    </div>
  );
}
