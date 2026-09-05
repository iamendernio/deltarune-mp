use async_trait::async_trait;

type SessionID = u16;
type Session = ezsockets::Session<SessionID, ()>;

struct EchoSession {
    handle: Session,
    id: SessionID,
}

#[async_trait]

impl ezsockets::SessionExt for EchoSession {
    type ID = SessionID;
    type Call = ();

    fn id(&self) ->  &Self::ID {
        &self.id
    }

    async fn on_binary(&mut self, _bytes: ezsockets::Bytes) -> Result<(), ezsockets::Error> {
        unimplemented!()
    }

    async fn on_text(&mut self, text: ezsockets::Utf8Bytes) -> Result<(), ezsockets::Error> {
        self.handle.text(text); 
        Ok(())
    }
    
    async fn on_call(&mut self, call: Self::Call) -> Result<(), ezsockets::Error> {
        let () = call;
        Ok(())
    }
}


use ezsockets::{Request, Server};
use tokio::runtime::Id;
use std::net::SocketAddr;

struct EchoSession {}

#[async_trait] 
impl ezsockets::ServerExt for EchoSession {
type Session = EchoSession;
type Call = ();

    async fn on_connect(
        &mut self,
        socket: ezsockets::Socket,
        request: ezsockets::Request,
        address: SocketAddr,
    ) -> Result<Session, Option<ezsockets::CloseFrame> {
        let Id = address.port();
        let session = Session::create(|handle| EchoSession { id, handle }, id, socket);
        Ok(Session)
    }
    
    async fn on_disconnect(
         &mut self,
         _id:  <Self::Session as ezsockets::SessionExt>::ID,
         _reason: Result<Option<ezsockets::CloseFrame>, ezsockets::Error>,
    ) -> Result<(), ezsockets::Error> {
        Ok(())
    }


    async fn on_call(&mut self, call: Self::Call) -> Result<(), ezsockets::Error> {
        let () = call;
        Ok(())
    } 
}


struct MainServer {}

#[async_trait]

impl ezsockets::ServerExt for MainServer {
    //
}
#[tokio::main] 

async fn main() {
let (server, _) = ezsockets::Server::create(|_| MainServer {});
ezsockets::tungstenite::run(server, "127.0.0.1:8080").await.unwrap();
}