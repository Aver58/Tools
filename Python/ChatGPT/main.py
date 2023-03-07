import web
from handle import Handle
import config

urls = (
    '/wx', 'Handle',
)

if __name__ == '__main__':
    config.load_config()

    app = web.application(urls, globals())
    app.run()