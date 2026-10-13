import { useEffect, useState } from "react";
import { Card, Tag } from "antd";
import axios from "axios";
import { useParams } from "react-router-dom";
import { statusColor } from "../utils/resultStatus";

export default function ExecutionResult(){

    const { id } = useParams();

    const [result,setResult]=useState<any>();

    useEffect(()=>{

        axios
            .get(`http://localhost:5072/api/results/${id}`)
            .then(res=>setResult(res.data));

    },[id]);

    if(!result)
        return null;

    return(

        <div style={{padding:20}}>

            <Card title={result.displayName}>

                <p>

                    <b>Status :</b>

                    <Tag color={statusColor(result.status)}>
                        {result.status}
                    </Tag>

                </p>

                <p>

                    <b>Executed : </b>

                    {result.status==="Not Run"
                        ? "-"
                        : new Date(result.executedAt).toLocaleString()}

                </p>

                <h3>Output</h3>

                <pre>{result.output}</pre>

                <h3>Error</h3>

                <pre>{result.error}</pre>

            </Card>

        </div>

    );

}