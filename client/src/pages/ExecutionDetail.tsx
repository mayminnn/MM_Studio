import { useEffect, useState } from "react";
import { Descriptions, Table, Tag } from "antd";
import axios from "axios";
import { useNavigate, useParams } from "react-router-dom";
import { statusColor } from "../utils/resultStatus";
import BackButton from "../components/BackButton";
import { getTestLinkIdMap, testLinkIdsFor } from "../services/testcaseService";
import { testLinkUrl } from "../utils/testLink";

type Result = {
    id: string;
    displayName: string;
    className: string;
    status: string;
    executedAt: string;
    suiteName: string;
    projectName?: string;
    version?: string;
    installerVersion?: string;
};

export default function ExecutionDetail() {

    const { runId } = useParams();

    const navigate = useNavigate();

    const [results, setResults] = useState<Result[]>([]);

    // TestLink IDs live on the test case library, keyed by class name
    const [testLinkIds, setTestLinkIds] = useState<Record<string, string[]>>({});

    useEffect(() => {

        axios
            .get(`http://localhost:5072/api/results/run/${runId}`)
            .then(res => setResults(res.data));

    }, [runId]);

    useEffect(() => {

        getTestLinkIdMap()
            .then(setTestLinkIds)
            .catch(() => setTestLinkIds({}));

    }, []);

    return (

        <div style={{ padding:20 }}>

            <BackButton to="/execution" label="Back" />

            <h2>Execution Details</h2>

            {results.length > 0 && (
                <Descriptions
                    bordered
                    size="small"
                    column={4}
                    style={{ marginBottom: 16 }}
                    items={[
                        { key: "suite", label: "Suite", children: results[0].suiteName || "-" },
                        // Blank for runs recorded before project/version were captured
                        { key: "project", label: "Project", children: results[0].projectName || "-" },
                        { key: "version", label: "Version", children: results[0].version || "-" },
                        { key: "installer", label: "Installer Version", children: results[0].installerVersion || "-" },
                    ]}
                />
            )}

            <Table
                rowKey="id"
                dataSource={results}
                onRow={(record)=>({
                    onClick:()=>{
                        navigate(`/executions/result/${record.id}`);
                    },
                    // Rows open the result, so make them look and act like links
                    onKeyDown:(e)=>{
                        // Ignore Enter on the TestLink links inside the row
                        if(e.key==="Enter" && e.target===e.currentTarget)
                            navigate(`/executions/result/${record.id}`);
                    },
                    tabIndex:0,
                    title:"View execution result",
                    style:{ cursor:"pointer" }
                })}
            >

                <Table.Column
                    title="Test ID"
                    dataIndex="className"
                />
                
                <Table.Column
                    title="Test"
                    dataIndex="displayName"
                />

                <Table.Column
                    title="TestLink ID"
                    render={(_,r:Result)=>{

                        const ids = testLinkIdsFor(testLinkIds, r.className);

                        return ids.length === 0
                            ? "-"
                            : ids.map(id =>
                                <Tag key={id}>
                                    {/* Stop the click from also opening the result row */}
                                    <a
                                        href={testLinkUrl(id)}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        onClick={e => e.stopPropagation()}
                                    >
                                        {id}
                                    </a>
                                </Tag>
                            );

                    }}
                />

                <Table.Column
                    title="Status"
                    render={(_,r:any)=>

                        <Tag color={statusColor(r.status)}>
                            {r.status}
                        </Tag>

                    }
                />

                <Table.Column
                    title="Executed"
                    render={(_,r:any)=>

                        r.status==="Not Run"
                            ? "-"
                            : new Date(r.executedAt).toLocaleString()

                    }
                />

                {/* Open hint */}
                <Table.Column
                    width={40}
                    align="center"
                    render={()=>
                        <span style={{ fontSize:18, color:"#999" }}>›</span>
                    }
                />

            </Table>

        </div>

    );

}

// import { useEffect, useState } from "react";
// import { useParams } from "react-router-dom";
// import { Table, Tag, Collapse } from "antd";
// import axios from "axios";

// type Result = {
//   id: string;
//   runId: string;
//   suiteName: string;
//   className: string;
//   displayName: string;
//   status: string;
//   output: string;
//   error: string;
//   executedAt: string;
// };

// export default function ExecutionDetails() {
//   const { runId } = useParams();
//   const [data, setData] = useState<Result[]>([]);

//   useEffect(() => {
//     axios
//       .get(`http://localhost:5072/api/results/run/${runId}`)
//       .then((res) => setData(res.data));
//   }, [runId]);

//   return (
//     <div style={{ padding: 20 }}>
//       <h2>Execution Details</h2>

//       <Table rowKey="id" dataSource={data} pagination={false}>
//         <Table.Column title="Test" dataIndex="displayName" />

//         <Table.Column
//           title="Status"
//           render={(_, r: Result) => (
//             <Tag color={r.status === "Passed" ? "green" : "red"}>
//               {r.status}
//             </Tag>
//           )}
//         />

//         <Table.Column
//           title="Logs"
//           render={(_, r: Result) => (
//             <Collapse
//               items={[
//                 {
//                   key: "1",
//                   label: "View Output",
//                   children: (
//                     <pre>
// {r.output}

// {r.error}
//                     </pre>
//                   ),
//                 },
//               ]}
//             />
//           )}
//         />
//       </Table>
//     </div>
//   );
// }