export default function ProfileForm() {
  return (
    <div className='flex flex-col gap-3 justify-between'>
      <div className="flex gap-5">
        <p className="p font-bold">SOL</p>
        <div className="flex">
          <p className="p pr-3">Account Address: </p>
          <p className="p">456f4d56sa4fd4saf2d3sa1f</p>
        </div>
        <p className="p ml-8">0.0</p>
      </div>
      <div className="flex gap-5">
        <p className="p font-bold">XRD</p>
        <div className="flex">
          <p className="p mr-3">Account Address: </p>
          <p className="p">456f4d56sa4fd4saf2d3sa1f</p>
        </div>
        <p className="p pl-8">0.0</p>
      </div>
    </div>
  )
}
