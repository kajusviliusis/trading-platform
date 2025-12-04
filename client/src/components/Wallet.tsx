import React, { useEffect, useState } from "react";
import { getWallet } from "../api";
import { Wallet } from "../types";

const WalletComponent: React.FC = () => {
    const [wallet, setWallet] = useState<Wallet | null>(null);

    useEffect(() => {
        getWallet().then(setWallet).catch(console.error);
    }, []);

    return (
        <div>
            <h2>Wallet</h2>
            {wallet ? (
                <p>
                    {wallet.balance} {wallet.currency}
                </p>
            ) : (
                <p>Loading...</p>
            )}
        </div>
    );
};

export default WalletComponent;
