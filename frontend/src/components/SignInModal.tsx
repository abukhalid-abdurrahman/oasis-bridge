import Modal from "./Modal";
import SignInForm from "./SignInForm";

export default function SignInModal() {
  return (
    <Modal className="relative">
      <h2 className="h2 mb-6">Sign In</h2>
      <SignInForm />
    </Modal>
  )
}
