"use client";

import { useEffect, useRef, useState } from "react";
import "ol/ol.css";
import Map from "ol/Map";
import View from "ol/View";
import TileLayer from "ol/layer/Tile";
import OSM from "ol/source/OSM";
import { fromLonLat, toLonLat } from "ol/proj";
import { Vector as VectorLayer } from "ol/layer";
import { Vector as VectorSource } from "ol/source";
import Feature from "ol/Feature";
import Point from "ol/geom/Point";
import { Icon, Style } from "ol/style";
import Modal from "./Modal";
import { Button } from "./ui/button";

interface LocationPickerModalProps {
  onSelect: (coords: { latitude: number; longitude: number }) => void;
  setIsOpen: (open: boolean) => void;
}

export default function LocationPickerModal({
  onSelect,
  setIsOpen,
}: LocationPickerModalProps) {
  const mapRef = useRef<HTMLDivElement>(null);
  const vectorSourceRef = useRef(new VectorSource());
  const mapObjRef = useRef<Map | null>(null);
  const [selectedCoords, setSelectedCoords] = useState<{
    latitude: number;
    longitude: number;
  } | null>(null);

  useEffect(() => {
    if (!mapRef.current) return;

    const rasterLayer = new TileLayer({
      source: new OSM(),
    });

    const markerLayer = new VectorLayer({
      source: vectorSourceRef.current,
    });

    const map = new Map({
      target: mapRef.current,
      layers: [rasterLayer, markerLayer],
      view: new View({
        center: fromLonLat([0, 0]),
        zoom: 2,
      }),
    });

    map.on("click", (event) => {
      const [lon, lat] = toLonLat(event.coordinate);
      setSelectedCoords({ latitude: lat, longitude: lon });

      vectorSourceRef.current.clear();

      const marker = new Feature({
        geometry: new Point(fromLonLat([lon, lat])),
      });

      marker.setStyle(
        new Style({
          image: new Icon({
            anchor: [0.5, 1],
            src: "/mapIcon.png",
            width: 30,
            height: 30
          }),
        })
      );

      vectorSourceRef.current.addFeature(marker);
    });

    mapObjRef.current = map;

    return () => {
      map.setTarget(null!);
    };
  }, []);

  const handleConfirm = () => {
    if (selectedCoords) {
      onSelect(selectedCoords);
      setIsOpen(false);
    }
  };

  return (
    <Modal isNonUrlModal onCloseFunc={() => setIsOpen(false)}>
      <div ref={mapRef} style={{ width: "100%", height: 400, borderRadius: 15, overflow: 'hidden' }} />
      <div className="mt-2 text-right">
        <Button
          onClick={handleConfirm}
          disabled={!selectedCoords}
          variant='gray'
          size='default'
          className="w-full"
        >
          Choose
        </Button>
      </div>
    </Modal>
  );
}
