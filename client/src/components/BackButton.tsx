import { Button } from "antd";
import { ArrowLeftOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";

type Props = {
  // Parent page to return to (fixed, so it works even when the page was opened directly)
  to: string;
  label?: string;
};

export default function BackButton({ to, label = "Back" }: Props) {
  const navigate = useNavigate();

  return (
    <Button
      type="text"
      icon={<ArrowLeftOutlined />}
      onClick={() => navigate(to)}
      style={{ marginBottom: 12, paddingLeft: 0 }}
    >
      {label}
    </Button>
  );
}
