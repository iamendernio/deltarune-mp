use async_trait::async_trait;
use ezsockets::{CloseFrame, Error, Server, ServerExt, Session, SessionExt};
use std::net::SocketAddr;
use std::sync::atomic::{AtomicU16, Ordering};

static NEXT_ID: AtomicU16 = AtomicU16::new(1);
pub struct EchoSession {
    pub id: u16,
    pub handle: ezsockets::Session<u16, ()>, // Handle для отправки сообщений
}

#[async_trait]

impl ezsockets::SessionExt for EchoSession {
    type ID = u16;
    type Call = ();

    fn id(&self) -> &Self::ID {
        &self.id
    }

    async fn on_binary(&mut self, _bytes: ezsockets::Bytes) -> Result<(), ezsockets::Error> {
        Ok(())
    }

    async fn on_text(&mut self, text: ezsockets::Utf8Bytes) -> Result<(), ezsockets::Error> {
        println!("Received from {:?} : {:?}", self.id, text);
        self.handle.text(text)?;

        Ok(())
    }

    async fn on_call(&mut self, call: Self::Call) -> Result<(), ezsockets::Error> {
        let () = call;
        Ok(())
    }
}

// #[async_trait]
// impl ezsockets::ServerExt for EchoSession {
//     type Session = EchoSession;
//     type Call = ();

//     async fn on_connect(
//         &mut self,
//         socket: ezsockets::Socket,
//         request: ezsockets::Request,
//         address: SocketAddr,
//     ) -> Result<Session, Option<ezsockets::CloseFrame>> {
//         let id = address.port();
//         let session = Session::create(|handle| EchoSession { id, handle }, id, socket);
//         Ok(session)
//     }

//     async fn on_disconnect(
//         &mut self,
//         _id: <Self::Session as ezsockets::SessionExt>::ID,
//         _reason: Result<Option<ezsockets::CloseFrame>, ezsockets::Error>,
//     ) -> Result<(), ezsockets::Error> {
//         Ok(())
//     }

//     async fn on_call(&mut self, call: Self::Call) -> Result<(), ezsockets::Error> {
//         let () = call;
//         Ok(())
//     }
// }

struct MainServer {}

#[async_trait]
impl ezsockets::ServerExt for MainServer {
    type Session = EchoSession;
    type Call = ();

    async fn on_connect(
        &mut self,
        socket: ezsockets::Socket,
        request: ezsockets::Request,
        address: SocketAddr,
    ) -> Result<
        ezsockets::Session<
            <Self::Session as ezsockets::SessionExt>::ID,
            <Self::Session as ezsockets::SessionExt>::Call,
        >,
        Option<ezsockets::CloseFrame>,
    > {
        let id = NEXT_ID.fetch_add(1, Ordering::SeqCst);
        let session = Session::create(|handle| EchoSession { handle, id }, id, socket);
        Ok(session)
    }

    async fn on_disconnect(
        &mut self,
        _id: <Self::Session as ezsockets::SessionExt>::ID,
        _reason: Result<Option<ezsockets::CloseFrame>, ezsockets::Error>,
    ) -> Result<(), ezsockets::Error> {
        print!("Disconnected: {}", _id);
        Ok(())
    }
    async fn on_call(&mut self, call: Self::Call) -> Result<(), Error> {
        Ok(())
    }
}

#[tokio::main]
async fn main() {
    let (server, _) = ezsockets::Server::create(|_| MainServer {});
    ezsockets::tungstenite::run(server, "127.0.0.1:8080")
        .await
        .unwrap();
}
