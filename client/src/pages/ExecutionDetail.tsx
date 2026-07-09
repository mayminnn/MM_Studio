import { useEffect, useState } from "react";
import { Table, Tag } from "antd";
import axios from "axios";
import { useNavigate, useParams } from "react-router-dom";

type Result = {
    id: string;
    displayName: string;
    className: string;
    status: string;
    executedAt: string;
};

export default function ExecutionDetail() {

    const { runId } = useParams();

    const navigate = useNavigate();

    const [results, setResults] = useState<Result[]>([]);

    useEffect(() => {

        axios
            .get(`http://localhost:5072/api/results/run/${runId}`)
            .then(res => setResults(res.data));

    }, [runId]);

    return (

        <div style={{ padding:20 }}>

            <h2>Execution Details</h2>

            <Table
                rowKey="id"
                dataSource={results}
                onRow={(record)=>({
                    onClick:()=>{
                        navigate(`/executions/result/${record.id}`);
                    }
                })}
            >

                <Table.Column
                    title="Test"
                    dataIndex="displayName"
                />

                <Table.Column
                    title="Status"
                    render={(_,r:any)=>

                        <Tag color={r.status==="Passed"?"green":"red"}>
                            {r.status}
                        </Tag>

                    }
                />

                <Table.Column
                    title="Executed"
                    render={(_,r:any)=>

                        new Date(r.executedAt).toLocaleString()

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