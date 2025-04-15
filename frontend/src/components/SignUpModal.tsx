import Modal from "./Modal";
import SignUpForm from "./SignUpForm";

export default function SignUpModal() {
  return (
    <Modal>
      <h2 className="h2 mb-6 text-black">Sign Up</h2>
      <SignUpForm />
    </Modal>
  )
}
