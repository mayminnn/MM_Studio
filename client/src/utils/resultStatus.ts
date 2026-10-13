// Statuses a test result can have: "Passed" | "Failed" | "Stopped" | "Not Run"

export const statusColor = (status: string) => {
  switch (status) {
    case "Passed": return "green";
    case "Failed": return "red";
    case "Stopped": return "orange";
    default: return "default";
  }
};

// Tests that were interrupted or never reached by a stopped run
export const wasNotRun = (status: string) =>
  status === "Stopped" || status === "Not Run";
