mod server;
use crate::server::MainServer;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error + Send + Sync>> {
    let (server, _) = ezsockets::Server::create(|_| MainServer {});
    ezsockets::tungstenite::run(server, "127.0.0.1:8080").await?;
    // println!("Hello, world!");
    Ok(())
}
